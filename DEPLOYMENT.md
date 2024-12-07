# EMRNext Deployment Guide

## Prerequisites
- Railway CLI
- Docker
- .NET 6.0 SDK
- Node.js 18+
- PostgreSQL

## Deployment Steps

### 1. Environment Configuration
1. Copy `.env.production` to Railway project variables
2. Set up secret management for sensitive credentials

### 2. Database Setup
- Use provided migration script `scripts/migrate-db.sh`
- Ensure PostgreSQL connection is configured

### 3. Railway Deployment
```bash
railway login
railway link
railway up
```

## Troubleshooting
- Check logs with `railway logs`
- Verify environment variables
- Ensure all services are connected

## Post-Deployment Checklist
- [ ] Validate API endpoints
- [ ] Run initial database seed
- [ ] Perform smoke tests
- [ ] Monitor application performance

## Security Recommendations
- Rotate JWT secrets regularly
- Use Railway's secret management
- Enable SSL/TLS
