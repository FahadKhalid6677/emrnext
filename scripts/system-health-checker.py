import os
import sys
import subprocess
import json
from datetime import datetime
import psutil
import requests

class SystemHealthChecker:
    def __init__(self):
        self.health_report = {
            "timestamp": datetime.now().isoformat(),
            "system_resources": {},
            "service_status": {},
            "network_connectivity": {},
            "security_checks": {},
            "overall_health": "UNKNOWN"
        }

    def check_system_resources(self):
        # CPU, Memory, Disk Usage
        self.health_report['system_resources'] = {
            "cpu_usage": psutil.cpu_percent(),
            "memory_usage": psutil.virtual_memory().percent,
            "disk_usage": psutil.disk_usage('/').percent
        }

    def check_service_status(self):
        # Check critical services
        services = [
            {"name": "backend", "url": "https://emrnext.railway.app/api/health"},
            {"name": "frontend", "url": "https://emrnext.railway.app"},
            {"name": "database", "url": "https://emrnext.railway.app/api/db-health"}
        ]

        for service in services:
            try:
                response = requests.get(service['url'], timeout=10)
                self.health_report['service_status'][service['name']] = {
                    "status": "HEALTHY" if response.status_code == 200 else "UNHEALTHY",
                    "response_code": response.status_code
                }
            except requests.RequestException:
                self.health_report['service_status'][service['name']] = {
                    "status": "UNREACHABLE",
                    "response_code": None
                }

    def check_network_connectivity(self):
        # Test external connectivity
        external_sites = [
            "https://www.google.com",
            "https://railway.app",
            "https://github.com"
        ]

        for site in external_sites:
            try:
                response = requests.get(site, timeout=5)
                self.health_report['network_connectivity'][site] = {
                    "status": "CONNECTED" if response.status_code == 200 else "DISCONNECTED",
                    "response_code": response.status_code
                }
            except requests.RequestException:
                self.health_report['network_connectivity'][site] = {
                    "status": "UNREACHABLE",
                    "response_code": None
                }

    def perform_security_checks(self):
        # Basic security checks
        try:
            # Check SSL certificate
            ssl_check = subprocess.run(
                ['openssl', 's_client', '-connect', 'emrnext.railway.app:443'], 
                capture_output=True, 
                text=True, 
                timeout=10
            )
            
            self.health_report['security_checks']['ssl_certificate'] = {
                "status": "VALID" if "Verification return code: 0" in ssl_check.stderr else "INVALID"
            }
        except subprocess.TimeoutExpired:
            self.health_report['security_checks']['ssl_certificate'] = {
                "status": "CHECK_FAILED"
            }

    def determine_overall_health(self):
        # Calculate overall system health
        health_criteria = [
            all(status['status'] == 'HEALTHY' for status in self.health_report['service_status'].values()),
            all(status['status'] == 'CONNECTED' for status in self.health_report['network_connectivity'].values()),
            self.health_report['system_resources']['cpu_usage'] < 80,
            self.health_report['system_resources']['memory_usage'] < 85,
            self.health_report['system_resources']['disk_usage'] < 90,
            all(check['status'] == 'VALID' for check in self.health_report['security_checks'].values())
        ]

        self.health_report['overall_health'] = (
            "EXCELLENT" if all(health_criteria) else
            "GOOD" if sum(health_criteria) >= 4 else
            "NEEDS_ATTENTION" if sum(health_criteria) >= 2 else
            "CRITICAL"
        )

    def generate_health_report(self):
        # Perform all health checks
        self.check_system_resources()
        self.check_service_status()
        self.check_network_connectivity()
        self.perform_security_checks()
        self.determine_overall_health()

        # Save report
        report_path = f'/var/log/emrnext/system_health_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'
        with open(report_path, 'w') as report_file:
            json.dump(self.health_report, report_file, indent=2)

        return self.health_report

def main():
    health_checker = SystemHealthChecker()
    report = health_checker.generate_health_report()
    print(json.dumps(report, indent=2))

if __name__ == "__main__":
    main()
