#!/bin/bash

echo "Initializing LocalStack resources..."

# Wait for LocalStack to be ready
until curl -s http://localhost:4566/_localstack/health | grep -q '"s3": "available"'; do
  echo "Waiting for LocalStack S3..."
  sleep 2
done

# Create S3 bucket
awslocal s3 mb s3://capitec-statement-bucket
echo "Created S3 bucket: capitec-statement-bucket"

# Verify SES email identity
awslocal ses verify-email-identity --email-address noreply@capitecbank.co.za
echo "Verified SES email: noreply@capitecbank.co.za"

# Create DynamoDB table for statements (if needed)
awslocal dynamodb create-table \
  --table-name Statements \
  --attribute-definitions AttributeName=AccountId,AttributeType=S AttributeName=StatementDate,AttributeType=S \
  --key-schema AttributeName=AccountId,KeyType=HASH AttributeName=StatementDate,KeyType=RANGE \
  --billing-mode PAY_PER_REQUEST \
  2>/dev/null || echo "Statements table may already exist"

echo "LocalStack initialization complete!"
