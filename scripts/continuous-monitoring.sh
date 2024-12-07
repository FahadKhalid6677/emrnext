#!/bin/bash

# Continuous Monitoring Script for EMRNext

# Monitoring Configuration
MONITOR_INTERVAL=300  # 5 minutes
MAX_RETRIES=3
ALERT_THRESHOLD=80    # Percentage for resource utilization alerts

# Logging Setup
LOG_DIR="/var/log/emrnext"
mkdir -p $LOG_DIR

# Performance Metrics Collection
collect_metrics() {
    local timestamp=$(date +"%Y-%m-%d %H:%M:%S")
    
    # CPU Usage
    local cpu_usage=$(top -bn1 | grep "Cpu(s)" | awk '{print $2 + $4}')
    
    # Memory Usage
    local memory_usage=$(free | grep Mem | awk '{print $3/$2 * 100.0}')
    
    # Disk Usage
    local disk_usage=$(df -h / | awk '/\// {print $(NF-1)}' | sed 's/%//')
    
    # Application Health Check
    local app_status=$(curl -s -o /dev/null -w "%{http_code}" https://emrnext.railway.app/health)
    
    # Log Metrics
    echo "$timestamp,CPU:$cpu_usage,Memory:$memory_usage,Disk:$disk_usage,AppStatus:$app_status" >> "$LOG_DIR/system_metrics.csv"
    
    # Alert on High Resource Utilization
    if (( $(echo "$cpu_usage > $ALERT_THRESHOLD" | bc -l) )) || 
       (( $(echo "$memory_usage > $ALERT_THRESHOLD" | bc -l) )) ||
       (( $(echo "$disk_usage > $ALERT_THRESHOLD" | bc -l) )) ||
       [[ "$app_status" != "200" ]]; then
        send_alert "$timestamp" "$cpu_usage" "$memory_usage" "$disk_usage" "$app_status"
    fi
}

# Alert Mechanism
send_alert() {
    local timestamp=$1
    local cpu_usage=$2
    local memory_usage=$3
    local disk_usage=$4
    local app_status=$5
    
    # Send email or SMS alert
    echo "ALERT: High Resource Utilization Detected
    Timestamp: $timestamp
    CPU Usage: $cpu_usage%
    Memory Usage: $memory_usage%
    Disk Usage: $disk_usage%
    App Status: $app_status" | mail -s "EMRNext Critical Alert" admin@emrnext.com
    
    # Optional: Trigger auto-scaling or self-healing
    railway scale --auto
}

# Main Monitoring Loop
main() {
    echo "Starting Continuous Monitoring for EMRNext"
    
    while true; do
        collect_metrics
        sleep $MONITOR_INTERVAL
    done
}

# Error Handling
trap 'echo "Monitoring interrupted. Restarting..."; sleep 5; main' ERR

# Start Monitoring
main
