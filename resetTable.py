import boto3
from botocore.exceptions import ClientError
import time

def delete_all_items(table_name, region_name='us-east-1'):
    """
    Delete all items from a DynamoDB table
    """
    dynamodb = boto3.resource('dynamodb', region_name=region_name)
    table = dynamodb.Table(table_name)
    
    key_schema = table.key_schema
    partition_key = next(key for key in key_schema if key['KeyType'] == 'HASH')['AttributeName']
    sort_key = None
    
    sort_key_elements = [key for key in key_schema if key['KeyType'] == 'RANGE']
    if sort_key_elements:
        sort_key = sort_key_elements[0]['AttributeName']
    
    print(f"Scanning table '{table_name}' for items to delete...")
    
    try:
        scan_response = table.scan()
        items = scan_response.get('Items', [])
        total_items = len(items)
        
        if total_items == 0:
            print(f"Table '{table_name}' is already empty.")
            return
            
        print(f"Found {total_items} items to delete in table '{table_name}'.")
        
        # Process paginated results
        while 'LastEvaluatedKey' in scan_response:
            scan_response = table.scan(ExclusiveStartKey=scan_response['LastEvaluatedKey'])
            items.extend(scan_response.get('Items', []))
            print(f"Found {len(scan_response.get('Items', []))} more items, total: {len(items)}")
        
        # Delete each item
        deleted_count = 0
        for item in items:
            key = {partition_key: item[partition_key]}
            if sort_key:
                key[sort_key] = item[sort_key]
            
            table.delete_item(Key=key)
            deleted_count += 1
            
            # Print progress periodically
            if deleted_count % 100 == 0 or deleted_count == total_items:
                print(f"Deleted {deleted_count}/{total_items} items...")
        
        print(f"Successfully deleted all {deleted_count} items from table '{table_name}'.")
        
    except ClientError as e:
        print(f"Error deleting items from table '{table_name}': {e}")


def reset_game_tables():
    """
    Reset both game leaderboard tables
    """
    tables = ['GameSessions', 'Players']
    
    print(f"\nResetting table '{table_name}'...")
    
    for table_name in tables:
        delete_all_items(table_name)
        
    print("\nAll game tables have been reset.")

if __name__ == "__main__":    
    reset_game_tables()