import unittest
import requests
import os
import time
from datetime import datetime

class TestMonitoringInfrastructure(unittest.TestCase):
    def setUp(self):
        self.prometheus_url = "http://localhost:9090"
        self.grafana_url = "http://localhost:3000"
        self.alertmanager_url = "http://localhost:9093"
        self.grafana_api_key = os.getenv("GRAFANA_API_KEY")
        
    def test_prometheus_health(self):
        """Test Prometheus health endpoint"""
        response = requests.get(f"{self.prometheus_url}/-/healthy")
        self.assertEqual(response.status_code, 200)
        
    def test_grafana_health(self):
        """Test Grafana health endpoint"""
        response = requests.get(f"{self.grafana_url}/api/health")
        self.assertEqual(response.status_code, 200)
        
    def test_alertmanager_health(self):
        """Test Alertmanager health endpoint"""
        response = requests.get(f"{self.alertmanager_url}/-/healthy")
        self.assertEqual(response.status_code, 200)
        
    def test_system_metrics_collection(self):
        """Test system metrics are being collected"""
        metrics = [
            "process_cpu_seconds_total",
            "process_resident_memory_bytes",
            "http_request_duration_milliseconds"
        ]
        
        for metric in metrics:
            response = requests.get(
                f"{self.prometheus_url}/api/v1/query",
                params={"query": metric}
            )
            self.assertEqual(response.status_code, 200)
            data = response.json()
            self.assertIn("data", data)
            self.assertIn("result", data["data"])
            
    def test_business_metrics_collection(self):
        """Test business metrics are being collected"""
        metrics = [
            "appointment_queue_depth",
            "claims_processed_total",
            "security_violation_total"
        ]
        
        for metric in metrics:
            response = requests.get(
                f"{self.prometheus_url}/api/v1/query",
                params={"query": metric}
            )
            self.assertEqual(response.status_code, 200)
            data = response.json()
            self.assertIn("data", data)
            
    def test_alert_rules(self):
        """Test alert rules are loaded"""
        response = requests.get(f"{self.prometheus_url}/api/v1/rules")
        self.assertEqual(response.status_code, 200)
        data = response.json()
        self.assertIn("data", data)
        self.assertIn("groups", data["data"])
        
    def test_grafana_dashboards(self):
        """Test Grafana dashboards are accessible"""
        headers = {"Authorization": f"Bearer {self.grafana_api_key}"}
        response = requests.get(
            f"{self.grafana_url}/api/dashboards/uid/system_metrics",
            headers=headers
        )
        self.assertEqual(response.status_code, 200)
        
        response = requests.get(
            f"{self.grafana_url}/api/dashboards/uid/business_metrics",
            headers=headers
        )
        self.assertEqual(response.status_code, 200)
        
    def test_alert_notification(self):
        """Test alert notification channels"""
        headers = {"Authorization": f"Bearer {self.grafana_api_key}"}
        response = requests.get(
            f"{self.grafana_url}/api/alert-notifications",
            headers=headers
        )
        self.assertEqual(response.status_code, 200)
        channels = response.json()
        self.assertTrue(len(channels) > 0)
        
    def test_data_retention(self):
        """Test data retention configuration"""
        response = requests.get(f"{self.prometheus_url}/api/v1/status/config")
        self.assertEqual(response.status_code, 200)
        config = response.json()
        self.assertIn("data", config)
        self.assertIn("storage", config["data"])
        
    def test_metric_timestamps(self):
        """Test metrics are being updated"""
        query = 'process_cpu_seconds_total'
        response = requests.get(
            f"{self.prometheus_url}/api/v1/query",
            params={"query": query}
        )
        self.assertEqual(response.status_code, 200)
        data = response.json()
        
        if "data" in data and "result" in data["data"] and len(data["data"]["result"]) > 0:
            timestamp = float(data["data"]["result"][0]["value"][0])
            current_time = time.time()
            # Check if metric is not older than 5 minutes
            self.assertLess(current_time - timestamp, 300)

if __name__ == '__main__':
    unittest.main()
