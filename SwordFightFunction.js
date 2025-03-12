const AWS = require('aws-sdk');
const dynamoDB = new AWS.DynamoDB.DocumentClient();

const TABLE_NAME = "SwordFightScores"; //  table name

exports.handler = async (event) => {
    try {
        // Set CORS headers
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
        
        // GET request - return leaderboard
        if (event.httpMethod === 'GET') {
            if (event.queryStringParameters && event.queryStringParameters.action === 'leaderboard') {
                const leaderboard = await getLeaderboard();
                return {
                    statusCode: 200,
                    headers,
                    body: JSON.stringify({ leaderboard })
                };
            }
            
            // Default GET response
            return {
                statusCode: 400,
                headers,
                body: JSON.stringify({ error: "Invalid GET request. Use ?action=leaderboard" })
            };
        }
        
        // POST request - submit score
        if (event.httpMethod === 'POST') {
            const requestBody = JSON.parse(event.body);
            
            if (!requestBody.playerId || requestBody.score === undefined) {
                return {
                    statusCode: 400,
                    headers,
                    body: JSON.stringify({ error: "Missing required fields: playerId and/or score" })
                };
            }
            
            await updatePlayerScore(requestBody.playerId, requestBody.score);
            
            return {
                statusCode: 200,
                headers,
                body: JSON.stringify({ message: "Score submitted successfully" })
            };
        }
        
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
            body: JSON.stringify({ error: "Internal server error" })
        };
    }
};

async function getLeaderboard() {
    const params = {
        TableName: TABLE_NAME,
        ProjectionExpression: "playerId, score",
        Limit: 100
    };
    
    const result = await dynamoDB.scan(params).promise();
    
    const sortedItems = result.Items.sort((a, b) => b.score - a.score);
    
    return sortedItems;
}

async function updatePlayerScore(playerId, newScore) {
    const getParams = {
        TableName: TABLE_NAME,
        Key: { playerId }
    };
    
    const existingRecord = await dynamoDB.get(getParams).promise();
    
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
    
    return dynamoDB.put(putParams).promise();
}