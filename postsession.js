// AWS Lambda Function - Submit Game Session
// This would be deployed as a Lambda function triggered by API Gateway
// Filename: submitGameSession.js

const AWS = require('aws-sdk');
const dynamoDB = new AWS.DynamoDB.DocumentClient();

exports.handler = async (event) => {
    try {
        // Parse request body
        const gameSession = JSON.parse(event.body);
        
        // Basic validation
        if (!gameSession.sessionId || !gameSession.player1Id || !gameSession.player2Id) {
            return {
                statusCode: 400,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: 'Missing required fields' })
            };
        }
        
        // Store the game session in DynamoDB
        await dynamoDB.put({
            TableName: 'GameSessions',
            Item: {
                sessionId: gameSession.sessionId,
                timestamp: gameSession.timestamp,
                player1Id: gameSession.player1Id,
                player2Id: gameSession.player2Id,
                player1Score: gameSession.player1Score,
                player2Score: gameSession.player2Score
            }
        }).promise();
        
        // Update player stats for both players
        await updatePlayerStats(gameSession.player1Id, gameSession.player1Score, gameSession.player1Score > gameSession.player2Score);
        await updatePlayerStats(gameSession.player2Id, gameSession.player2Score, gameSession.player2Score > gameSession.player1Score);
        
        return {
            statusCode: 200,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: 'Game session recorded successfully' })
        };
    } catch (error) {
        console.error('Error processing game session:', error);
        return {
            statusCode: 500,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: 'Internal server error' })
        };
    }
};

async function updatePlayerStats(playerId, score, isWin) {
    // Get current player stats
    const playerData = await dynamoDB.get({
        TableName: 'Players',
        Key: { playerId }
    }).promise();
    
    // Create or update player record
    const player = playerData.Item || {
        playerId,
        playerName: 'Unknown Player', // Should be set during registration
        totalWins: 0,
        totalMatches: 0,
        totalScore: 0
    };
    
    // Update stats
    player.totalMatches += 1;
    player.totalScore += score;
    if (isWin) {
        player.totalWins += 1;
    }
    
    // Save updated stats
    await dynamoDB.put({
        TableName: 'Players',
        Item: player
    }).promise();
}

// AWS Lambda Function - Get Leaderboard
// Filename: getLeaderboard.js

exports.handler = async (event) => {
    try {
        // Scan the Players table to get all players
        const result = await dynamoDB.scan({
            TableName: 'Players'
        }).promise();
        
        // Sort players by wins (you could add more complex sorting logic)
        const sortedPlayers = result.Items.sort((a, b) => {
            // Primary sort by wins
            if (b.totalWins !== a.totalWins) {
                return b.totalWins - a.totalWins;
            }
            // Secondary sort by total score
            return b.totalScore - a.totalScore;
        });
        
        // Limit to top players if needed
        const topPlayers = sortedPlayers.slice(0, 100);
        
        return {
            statusCode: 200,
            headers: { 
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*' // Allow cross-origin requests
            },
            body: JSON.stringify(topPlayers)
        };
    } catch (error) {
        console.error('Error fetching leaderboard:', error);
        return {
            statusCode: 500,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: 'Internal server error' })
        };
    }
};

// AWS Lambda Function - Get Player Stats
// Filename: getPlayerStats.js

exports.handler = async (event) => {
    try {
        const playerId = event.pathParameters.playerId;
        
        // Get player stats from DynamoDB
        const result = await dynamoDB.get({
            TableName: 'Players',
            Key: { playerId }
        }).promise();
        
        if (!result.Item) {
            return {
                statusCode: 404,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: 'Player not found' })
            };
        }
        
        return {
            statusCode: 200,
            headers: { 
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            body: JSON.stringify(result.Item)
        };
    } catch (error) {
        console.error('Error fetching player stats:', error);
        return {
            statusCode: 500,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: 'Internal server error' })
        };
    }
};