#!/bin/bash

set -e

echo "Waiting for LocalStack to be ready..."
until curl -s http://localhost:4566/_localstack/health | grep -q '"ses": "available"'; do
  echo "Waiting for LocalStack SES service..."
  sleep 2
done

echo "Verifying email address in LocalStack SES..."
aws --endpoint-url=http://localhost:4566 ses verify-email-identity --email-address noreply@example.com --region us-east-1

echo "Email address noreply@example.com verified successfully!"
