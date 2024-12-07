# EMRNext Monitoring Deployment Guide

## Prerequisites
- Docker and Docker Compose installed
- Access to SMTP server for notifications
- PagerDuty account (for critical alerts)
- Network access to monitoring ports (3000, 9090, 9093)

## Environment Setup

1. Set required environment variables:
```bash
export GRAFANA_ADMIN_PASSWORD=<secure-password>
export SMTP_USER=<email-username>
export SMTP_PASSWORD=<email-password>
export PAGERDUTY_SERVICE_KEY=<pagerduty-key>
```

2. Create necessary directories:
```bash
mkdir -p /opt/emrnext/monitoring/{dashboards,grafana-provisioning}
```

## Deployment Steps

1. **Deploy Monitoring Stack**
```bash
cd /opt/emrnext/monitoring
docker-compose up -d
```

2. **Verify Services**
```bash
# Check service status
docker-compose ps

# Verify Prometheus
curl http://localhost:9090/-/healthy

# Verify Grafana
curl http://localhost:3000/api/health

# Verify Alertmanager
curl http://localhost:9093/-/healthy
```

3. **Import Dashboards**
- Access Grafana at http://localhost:3000
- Login with admin credentials
- Import system-metrics.json and business-metrics.json

4. **Configure Alert Channels**
- Set up email notifications in Grafana
- Configure PagerDuty integration
- Test notification delivery

## Validation Checklist

- [ ] All services running
- [ ] Metrics being collected
- [ ] Dashboards accessible
- [ ] Alerts configured
- [ ] Notifications working
- [ ] Data retention set
- [ ] Backup configured

## Security Considerations

1. **Access Control**
- Use strong passwords
- Enable HTTPS
- Implement authentication
- Configure firewalls

2. **Data Protection**
- Encrypt sensitive data
- Secure backup storage
- Monitor access logs
- Regular security audits

## Maintenance Procedures

1. **Regular Tasks**
- Monitor disk usage
- Review alert thresholds
- Update configurations
- Backup dashboards
- Rotate logs

2. **Troubleshooting**
- Check service logs
- Verify connectivity
- Review alert history
- Validate metrics
- Test notifications

## Emergency Procedures

1. **Service Failure**
- Check container logs
- Restart services
- Verify data integrity
- Restore from backup
- Update documentation

2. **Alert Storm**
- Identify root cause
- Silence redundant alerts
- Fix underlying issues
- Update thresholds
- Document incidents

## Contact Information

- Technical Support: support@emrnext.com
- Security Team: security@emrnext.com
- Operations: ops@emrnext.com

## References
- Prometheus Documentation
- Grafana Documentation
- AlertManager Documentation
- Docker Documentation
- Security Guidelines
