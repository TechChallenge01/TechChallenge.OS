#!/usr/bin/env bash
# Execute UMA VEZ antes do primeiro `terraform init`.
# Cria o bucket S3 (backend de estado) e a tabela DynamoDB (lock).
#
# Pré-requisito: AWS CLI configurado com credenciais de admin.
set -euo pipefail

AWS_REGION="sa-east-1"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
BUCKET_NAME="techchallenge-tfstate-${ACCOUNT_ID}"
TABLE_NAME="techchallenge-tflock"

echo "==> Criando bucket S3: ${BUCKET_NAME}"
aws s3api create-bucket \
  --bucket "${BUCKET_NAME}" \
  --region "${AWS_REGION}" \
  --create-bucket-configuration LocationConstraint="${AWS_REGION}"

aws s3api put-bucket-versioning \
  --bucket "${BUCKET_NAME}" \
  --versioning-configuration Status=Enabled

aws s3api put-bucket-encryption \
  --bucket "${BUCKET_NAME}" \
  --server-side-encryption-configuration \
  '{"Rules":[{"ApplyServerSideEncryptionByDefault":{"SSEAlgorithm":"AES256"}}]}'

aws s3api put-public-access-block \
  --bucket "${BUCKET_NAME}" \
  --public-access-block-configuration \
  "BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true"

echo "==> Criando tabela DynamoDB: ${TABLE_NAME}"
aws dynamodb create-table \
  --table-name "${TABLE_NAME}" \
  --attribute-definitions AttributeName=LockID,AttributeType=S \
  --key-schema AttributeName=LockID,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region "${AWS_REGION}"

echo ""
echo "==> Pronto! Atualize o bucket em providers.tf:"
echo "    bucket = \"${BUCKET_NAME}\""
echo ""
echo "==> Adicione ao GitHub Secrets:"
echo "    AWS_BACKEND_BUCKET = ${BUCKET_NAME}"
