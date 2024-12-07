# EMRNext Advanced Troubleshooting Guide

## Table of Contents
1. [System Health Checks](#system-health-checks)
2. [Performance Issues](#performance-issues)
3. [Security Alerts](#security-alerts)
4. [Database Issues](#database-issues)
5. [Caching Problems](#caching-problems)
6. [Deployment Failures](#deployment-failures)

## System Health Checks

### Monitoring Dashboard
1. Access Grafana at `http://your-domain:3000`
2. Navigate to "EMRNext Performance Metrics"
3. Check for any red indicators or alerts

### Common Health Check Issues
```bash
# Check service status
kubectl get pods -n emrnext

# View service logs
kubectl logs -f deployment/emrnext -n emrnext

# Check resource usage
kubectl top pods -n emrnext
```

## Performance Issues

### High CPU Usage
1. Check Prometheus metrics
2. Analyze slow queries in database
3. Review caching effectiveness

```sql
-- Find slow queries
SELECT query, 
       round(total_time::numeric, 2) as total_time,
       calls,
       round(mean_time::numeric, 2) as mean,
       round((100 * total_time / sum(total_time::numeric) over ())::numeric, 2) as percentage_cpu
FROM pg_stat_statements
ORDER BY total_time DESC
LIMIT 10;
```

### Memory Leaks
1. Use dotnet-trace to capture memory dumps
2. Analyze with dotnet-dump
3. Review object allocation patterns

```bash
# Capture memory dump
dotnet-trace collect --process-id <PID>

# Analyze dump
dotnet-dump analyze core_XXXXXX
```

## Security Alerts

### Handling Security Incidents
1. Check security logs
2. Review threat detection alerts
3. Analyze audit trails

```bash
# View security logs
kubectl logs -f deployment/security-monitor -n emrnext

# Check audit logs
kubectl logs -f deployment/audit-service -n emrnext
```

## Database Issues

### Connection Problems
1. Verify connection strings
2. Check network connectivity
3. Validate credentials

### Data Inconsistency
1. Run data validation scripts
2. Check replication status
3. Verify backup integrity

```sql
-- Check replication status
SELECT * FROM pg_stat_replication;

-- Verify connection count
SELECT count(*) FROM pg_stat_activity;
```

## Caching Problems

### Cache Invalidation
1. Monitor cache hit rates
2. Check cache size and memory usage
3. Verify cache consistency

```bash
# Redis cache monitoring
redis-cli info | grep used_memory

# Check cache keys
redis-cli keys "*"
```

## Deployment Failures

### Blue-Green Deployment Issues
1. Check deployment status
2. Verify health checks
3. Review rollout history

```bash
# Check deployment status
kubectl rollout status deployment/emrnext-blue -n emrnext

# View rollout history
kubectl rollout history deployment/emrnext-blue -n emrnext

# Rollback if needed
kubectl rollout undo deployment/emrnext-blue -n emrnext
```

### Common Deployment Fixes
1. Verify resource limits
2. Check image versions
3. Validate configurations

```bash
# Check pod events
kubectl describe pod <pod-name> -n emrnext

# Verify configmaps
kubectl get configmaps -n emrnext
```

## Performance Tuning

### Application Performance
1. Enable detailed logging
2. Monitor API response times
3. Review resource utilization

### Database Performance
1. Analyze query plans
2. Check index usage
3. Monitor connection pools

```sql
-- Check index usage
SELECT schemaname, tablename, indexname, idx_scan, idx_tup_read, idx_tup_fetch
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;
```

## Contact Support
For additional assistance:
- Email: support@emrnext.com
- Slack: #emrnext-support
- Emergency: +1-XXX-XXX-XXXX
