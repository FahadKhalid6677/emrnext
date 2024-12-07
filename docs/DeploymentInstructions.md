# EMRNext Deployment Instructions

## Prerequisites
- Docker and Docker Compose installed on the production server
- SSL certificates for HTTPS
- Access to production environment
- Required environment variables

## Pre-Deployment Steps

1. **Environment Setup**
   ```bash
   # Copy environment template
   cp .env.template .env
   
   # Edit .env file with production values
   nano .env
   ```

2. **SSL Certificate Setup**
   - Place SSL certificates in a secure location
   - Update CERT_PATH in .env to point to certificates
   - Ensure certificates have proper permissions

3. **Database Backup (if updating existing deployment)**
   ```bash
   # Backup existing database
   docker exec emrnext-db /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U SA -P "$DB_PASSWORD" \
     -Q "BACKUP DATABASE EMRNext TO DISK = '/var/opt/mssql/backup/emrnext.bak'"
   ```

## Deployment Steps

1. **Build and Start Services**
   ```bash
   # Pull latest changes
   git pull origin main

   # Build and start services
   docker-compose -f docker-compose.prod.yml build
   docker-compose -f docker-compose.prod.yml up -d
   ```

2. **Verify Deployment**
   ```bash
   # Check service status
   docker-compose -f docker-compose.prod.yml ps

   # Check logs
   docker-compose -f docker-compose.prod.yml logs -f
   ```

3. **Run Database Migrations**
   ```bash
   # Execute migrations
   docker-compose -f docker-compose.prod.yml exec api \
     dotnet ef database update
   ```

4. **Health Check**
   - Verify API health: https://api.emrnext.com/health
   - Check Seq logs: http://localhost:5341
   - Test key functionality through the web interface

## Post-Deployment Steps

1. **Verify Security**
   - Confirm HTTPS is working
   - Test authentication
   - Verify API endpoints are secure
   - Check CORS settings

2. **Monitor Performance**
   - Watch application logs in Seq
   - Monitor system resources
   - Check response times
   - Verify caching is working

3. **Backup Verification**
   - Verify database backups are running
   - Test backup restoration process
   - Confirm data persistence

## Rollback Procedure

If issues are encountered:

1. **Stop New Services**
   ```bash
   docker-compose -f docker-compose.prod.yml down
   ```

2. **Restore Database (if needed)**
   ```bash
   # Restore from backup
   docker exec emrnext-db /opt/mssql-tools/bin/sqlcmd \
     -S localhost -U SA -P "$DB_PASSWORD" \
     -Q "RESTORE DATABASE EMRNext FROM DISK = '/var/opt/mssql/backup/emrnext.bak'"
   ```

3. **Deploy Previous Version**
   ```bash
   # Checkout previous version
   git checkout [previous-tag]
   
   # Rebuild and start
   docker-compose -f docker-compose.prod.yml up -d --build
   ```

## Maintenance

1. **Regular Tasks**
   - Monitor logs daily
   - Check system resources
   - Review security alerts
   - Verify backup integrity
   - Update SSL certificates before expiry

2. **Scaling**
   ```bash
   # Scale specific services
   docker-compose -f docker-compose.prod.yml up -d --scale api=3
   ```

3. **Updates**
   - Schedule maintenance windows
   - Notify users of downtime
   - Follow rollback procedure if issues occur

## Support

For deployment issues:
- Check logs in Seq dashboard
- Review application logs
- Contact DevOps team
- Refer to troubleshooting guide

## Security Notes

- Keep .env file secure and never commit to repository
- Regularly rotate database passwords
- Monitor for suspicious activity
- Keep all containers updated with security patches
- Regular security audits
