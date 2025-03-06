import boto3
from botocore.exceptions import ClientError
import uuid
from datetime import datetime

class LeaderboardManager:    
    def __init__(self, region_name='us-east-1'):
        self.dynamodb = boto3.resource('dynamodb', region_name=region_name)
        self.sessions_table = self.dynamodb.Table('GameSessions')
        self.players_table = self.dynamodb.Table('Players')
    
    def record_game_session(self, player1_id, player2_id, player1_score, player2_score):
        session_id = str(uuid.uuid4())
        timestamp = datetime.now().isoformat() + 'Z'
        
        # Create session record
        session_data = {
            'sessionId': session_id,
            'timestamp': timestamp,
            'player1Id': player1_id,
            'player2Id': player2_id,
            'player1Score': player1_score,
            'player2Score': player2_score
        }
        
        try:
            self.sessions_table.put_item(Item=session_data)
            print(f"Game session {session_id} recorded successfully")
            
            # Update player statistics
            player1_won = player1_score > player2_score
            player2_won = player2_score > player1_score
            
            self._update_player_stats(player1_id, player1_score, player1_won)
            self._update_player_stats(player2_id, player2_score, player2_won)
            
            return session_id
        except ClientError as e:
            print(f"Error recording game session: {e}")
            return None
    
    def _update_player_stats(self, player_id, score, is_win):
        """Update a player's statistics after a game"""
        try:
            # Try to get existing player record
            response = self.players_table.get_item(Key={'playerId': player_id})
            
            if 'Item' in response:
                # Player exists, update stats
                player = response['Item']
                
                # Update using atomic counters
                self.players_table.update_item(
                    Key={'playerId': player_id},
                    UpdateExpression='SET totalMatches = totalMatches + :inc, '
                                     'totalScore = totalScore + :score, '
                                     'totalWins = totalWins + :win',
                    ExpressionAttributeValues={
                        ':inc': 1,
                        ':score': score,
                        ':win': 1 if is_win else 0
                    }
                )
            else:
                # Player doesn't exist, create new record
                player = {
                    'playerId': player_id,
                    'playerName': f'Player {player_id[:8]}',  # Default name
                    'totalMatches': 1,
                    'totalScore': score,
                    'totalWins': 1 if is_win else 0
                }
                self.players_table.put_item(Item=player)
                
            print(f"Updated stats for player {player_id}")
            
        except ClientError as e:
            print(f"Error updating player stats: {e}")
    
    def get_leaderboard(self, limit=100):
        """Get the top players ranked by wins"""
        try:
            # Scan the players table
            response = self.players_table.scan()
            players = response['Items']
            
            # Sort by wins (primary) and score (secondary)
            sorted_players = sorted(
                players,
                key=lambda x: (x.get('totalWins', 0), x.get('totalScore', 0)),
                reverse=True
            )
            
            # Return top N players
            return sorted_players[:limit]
            
        except ClientError as e:
            print(f"Error retrieving leaderboard: {e}")
            return []
    
    def get_player_stats(self, player_id):
        """Get statistics for a specific player"""
        try:
            response = self.players_table.get_item(Key={'playerId': player_id})
            
            if 'Item' in response:
                return response['Item']
            else:
                print(f"Player {player_id} not found")
                return None
                
        except ClientError as e:
            print(f"Error retrieving player stats: {e}")
            return None
    
    def get_player_match_history(self, player_id, limit=10):
        """Get recent matches for a player"""
        try:
            # For this, we need to scan the GameSessions table and filter
            # This is not the most efficient way but works for small datasets
            response = self.sessions_table.scan(
                FilterExpression='player1Id = :pid OR player2Id = :pid',
                ExpressionAttributeValues={':pid': player_id}
            )
            
            matches = response['Items']
            
            # Sort by timestamp (most recent first)
            sorted_matches = sorted(
                matches,
                key=lambda x: x.get('timestamp', ''),
                reverse=True
            )
            
            return sorted_matches[:limit]
            
        except ClientError as e:
            print(f"Error retrieving match history: {e}")
            return []
