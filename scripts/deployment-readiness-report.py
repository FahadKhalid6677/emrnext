import os
import json
from datetime import datetime

class DeploymentReadinessReport:
    def __init__(self):
        self.report = {
            "timestamp": datetime.now().isoformat(),
            "system_components": {
                "backend": {"status": "Not Verified", "version": None},
                "frontend": {"status": "Not Verified", "version": None},
                "database": {"status": "Not Verified", "version": None}
            },
            "deployment_metrics": {
                "total_deployment_time": None,
                "successful_stages": 0,
                "failed_stages": 0
            },
            "recommendations": []
        }

    def load_deployment_logs(self, log_path):
        try:
            with open(log_path, 'r') as log_file:
                logs = log_file.read()
                # Extract versions and statuses
                self.report['system_components']['backend']['version'] = self._extract_version(logs, 'backend')
                self.report['system_components']['frontend']['version'] = self._extract_version(logs, 'frontend')
                self.report['system_components']['database']['version'] = self._extract_version(logs, 'database')
        except Exception as e:
            print(f"Error loading deployment logs: {e}")

    def _extract_version(self, logs, component):
        # Implement version extraction logic
        version_patterns = {
            'backend': r'Backend Version: ([\d.]+)',
            'frontend': r'Frontend Version: ([\d.]+)',
            'database': r'Database Version: ([\d.]+)'
        }
        import re
        match = re.search(version_patterns.get(component, ''), logs)
        return match.group(1) if match else "Unknown"

    def analyze_deployment_performance(self, metrics_path):
        try:
            with open(metrics_path, 'r') as metrics_file:
                metrics = json.load(metrics_file)
                
                # Calculate total deployment time
                start_times = [stage['start'] for stage in metrics['stages'].values()]
                end_times = [stage['end'] for stage in metrics['stages'].values()]
                
                self.report['deployment_metrics']['total_deployment_time'] = (
                    datetime.fromisoformat(max(end_times)) - 
                    datetime.fromisoformat(min(start_times))
                ).total_seconds()
                
                # Count successful and failed stages
                self.report['deployment_metrics']['successful_stages'] = len(
                    [stage for stage in metrics['stages'].values() if stage.get('status') == 'Success']
                )
                self.report['deployment_metrics']['failed_stages'] = len(
                    [stage for stage in metrics['stages'].values() if stage.get('status') != 'Success']
                )
        except Exception as e:
            print(f"Error analyzing deployment performance: {e}")

    def generate_recommendations(self):
        # Generate deployment recommendations based on analysis
        recommendations = []
        
        if self.report['deployment_metrics']['failed_stages'] > 0:
            recommendations.append(
                "Review and optimize deployment stages with failures"
            )
        
        if any(component['status'] != 'Verified' for component in self.report['system_components'].values()):
            recommendations.append(
                "Manually verify system component statuses"
            )
        
        self.report['recommendations'] = recommendations

    def create_report(self):
        self.load_deployment_logs('/var/log/emrnext/deployment.log')
        self.analyze_deployment_performance('/var/log/emrnext/deployment_metrics.json')
        self.generate_recommendations()
        
        # Write report to file
        report_path = f'/var/log/emrnext/deployment_report_{datetime.now().strftime("%Y%m%d_%H%M%S")}.json'
        with open(report_path, 'w') as report_file:
            json.dump(self.report, report_file, indent=2)
        
        return self.report

def main():
    readiness_report = DeploymentReadinessReport()
    final_report = readiness_report.create_report()
    print(json.dumps(final_report, indent=2))

if __name__ == "__main__":
    main()
