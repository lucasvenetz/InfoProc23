const { DynamoDBClient } = require('@aws-sdk/client-dynamodb');
const { DynamoDBDocumentClient, ScanCommand, GetCommand, PutCommand } = require('@aws-sdk/lib-dynamodb');

const client = new DynamoDBClient({});
const dynamoDB = DynamoDBDocumentClient.from(client);

const TABLE_NAME = "SwordFightScores"; 

async function getLeaderboard() {
    const params = {
        TableName: TABLE_NAME,
        ProjectionExpression: "playerId, score",
        Limit: 100
    };
    
    const command = new ScanCommand(params);
    const result = await dynamoDB.send(command);
    
    const sortedItems = result.Items.sort((a, b) => b.score - a.score);
    
    return sortedItems;
}

async function updatePlayerScore(playerId, newScore) {
    const getParams = {
        TableName: TABLE_NAME,
        Key: { playerId }
    };
    
    const getCommand = new GetCommand(getParams);
    const existingRecord = await dynamoDB.send(getCommand);
    
    // If player exists and new score is not higher, don't update
    if (existingRecord.Item && existingRecord.Item.score >= newScore) {
        console.log(`Player ${playerId} already has a higher score (${existingRecord.Item.score})`);
        return;
    }
    
    const putParams = {
        TableName: TABLE_NAME,
        Item: {
            playerId,
            score: newScore,
            timestamp: new Date().toISOString()
        }
    };
    
    const putCommand = new PutCommand(putParams);
    return dynamoDB.send(putCommand);
}

// Main handler function
exports.lambda_handler = async (event) => {
    try {
        console.log("Received event:", JSON.stringify(event));
        
        const headers = {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Headers": "Content-Type",
            "Access-Control-Allow-Methods": "OPTIONS,POST,GET"
        };
        
        // Handle OPTIONS request (preflight)
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
        
        // POST request - submit score
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
            
            // Submit score
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