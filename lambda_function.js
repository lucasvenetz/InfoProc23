const { DynamoDBClient } = require('@aws-sdk/client-dynamodb');
const { DynamoDBDocumentClient, ScanCommand, GetCommand, PutCommand } = require('@aws-sdk/lib-dynamodb');

const client = new DynamoDBClient({});
const dynamoDB = DynamoDBDocumentClient.from(client);

const TABLE_NAME = "SwordFightScores"; 

async function getLeaderboard() {
    const params = {
        TableName: TABLE_NAME,
        ProjectionExpression: "playerId, score, totalScore, gamesPlayed"
    };
    
    const command = new ScanCommand(params);
    const result = await dynamoDB.send(command);
    
    const sortedItems = result.Items.sort((a, b) => (b.totalScore || 0) - (a.totalScore || 0));
    
    return sortedItems;
}

// add score to player's total score if playerID already exists
async function updatePlayerScore(playerId, newScore) {
    const currentTime = Math.floor(Date.now() / 1000);
    const timeLimit = 3600;

    const getParams = {
        TableName: TABLE_NAME,
        Key: { playerId }
    };
    
    const getCommand = new GetCommand(getParams);
    const existingRecord = await dynamoDB.send(getCommand);
    
    let highestScore = newScore;
    let totalScore = newScore;
    let gamesPlayed = 1;
    
    if (existingRecord.Item) {
        highestScore = Math.max(existingRecord.Item.score || 0, newScore);
        totalScore = (existingRecord.Item.totalScore || 0) + newScore;
        gamesPlayed = (existingRecord.Item.gamesPlayed || 0) + 1;
    }

    const updateItem = {
        playerId,
        score: highestScore,
        totalScore: totalScore,
        gamesPlayed: gamesPlayed,
        timestamp: new Date().toISOString()
    };

    // TTL if score is zero - expire 1 hour
    if (totalScore == 0) {
        updateItem.ttl = currentTime + timeLimit; 
    }
    
    const putParams = {
        TableName: TABLE_NAME,
        Item: updateItem
    };
    
    const putCommand = new PutCommand(putParams);
    return dynamoDB.send(putCommand);
}

exports.lambda_handler = async (event) => {
    try {
        console.log("Received event:", JSON.stringify(event));
        
        const headers = {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Headers": "Content-Type",
            "Access-Control-Allow-Methods": "OPTIONS,POST,GET"
        };
        
        if (event.httpMethod === 'OPTIONS') {
            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({ message: "CORS preflight response" })
            };
        }
        
        if (event.httpMethod === 'GET') {
            console.log("Processing GET request");
            
            if (event.queryStringParameters && event.queryStringParameters.action === 'leaderboard') {
                console.log("Fetching leaderboard");
                const leaderboard = await getLeaderboard();
                console.log("Leaderboard fetched:", JSON.stringify(leaderboard));
                
                return {
                    statusCode: 200,
                    headers,
                    body: JSON.stringify({ leaderboard })
                };
            }
            
            console.log("Invalid GET request - missing action=leaderboard parameter");
            return {
                statusCode: 400,
                headers,
                body: JSON.stringify({ error: "Invalid GET request. Use ?action=leaderboard" })
            };
        }
        
        if (event.httpMethod === 'POST') {
            console.log("Processing POST request");
            
            const requestBody = JSON.parse(event.body);
            console.log("Request body:", JSON.stringify(requestBody));
            
            if (!requestBody.playerId || requestBody.score === undefined) {
                console.log("Invalid POST request - missing required fields");
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({ error: "Missing required fields: playerId and/or score" })
                };
            }
            
            console.log(`Updating score for player ${requestBody.playerId}: ${requestBody.score}`);
            await updatePlayerScore(requestBody.playerId, requestBody.score);
            console.log("Score updated successfully");
            
            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({ message: "Score submitted successfully" })
            };
        }
        
        console.log(`Unsupported HTTP method: ${event.httpMethod}`);
        return {
            statusCode: 405,
            headers,
            body: JSON.stringify({ error: "Method not allowed" })
        };
    } catch (error) {
        console.error("Error:", error);
        return {
            statusCode: 500,
            headers: {
                "Access-Control-Allow-Origin": "*"
            },
            body: JSON.stringify({ error: "Internal server error", details: error.message })
        };
    }
};