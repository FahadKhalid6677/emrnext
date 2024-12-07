import os
import re
import json
from datetime import datetime
import logging

class DeploymentAnalyzer:
    def __init__(self, log_file):
        self.log_file = log_file
        self.deployment_metrics = {
            "start_time": None,
            "end_time": None,
            "total_duration": None,
            "stages": {},
            "errors": [],
            "status": "Pending"
        }
        logging.basicConfig(filename='/var/log/emrnext/deployment_analysis.log', level=logging.INFO)

    def parse_deployment_log(self):
        try:
            with open(self.log_file, 'r') as file:
                log_content = file.read()
                
                # Extract deployment stages and timings
                stage_pattern = r'\[DEPLOY\] (.*?) - Started at (.*?) - Completed at (.*?)'
                stages = re.findall(stage_pattern, log_content)
                
                for stage, start, end in stages:
                    self.deployment_metrics['stages'][stage] = {
                        'start': start,
                        'end': end,
                        'duration': self._calculate_duration(start, end)
                    }
                
                # Detect errors
                error_pattern = r'ERROR: (.*?)'
                self.deployment_metrics['errors'] = re.findall(error_pattern, log_content)
                
                # Determine overall status
                self.deployment_metrics['status'] = (
                    'Success' if not self.deployment_metrics['errors'] 
                    else 'Partial Failure' if len(self.deployment_metrics['errors']) < 3 
                    else 'Critical Failure'
                )
                
                # Log analysis results
                logging.info(f"Deployment Analysis: {json.dumps(self.deployment_metrics, indent=2)}")
                
                return self.deployment_metrics

    def _calculate_duration(self, start, end):
        try:
            start_time = datetime.strptime(start, '%Y-%m-%d %H:%M:%S')
            end_time = datetime.strptime(end, '%Y-%m-%d %H:%M:%S')
            return (end_time - start_time).total_seconds()
        except Exception as e:
            logging.error(f"Duration calculation error: {e}")
            return None

    def generate_deployment_report(self):
        report = f"""
# EMRNext Deployment Analysis Report

## Deployment Status: {self.deployment_metrics['status']}

### Deployment Stages:
{json.dumps(self.deployment_metrics['stages'], indent=2)}

### Errors Detected:
{', '.join(self.deployment_metrics['errors']) or 'No errors'}

### Recommendations:
1. Review stages with longer durations
2. Investigate any detected errors
3. Consider optimization for future deployments
"""
        
        with open('/var/log/emrnext/deployment_report.md', 'w') as report_file:
            report_file.write(report)
        
        return report

def main():
    analyzer = DeploymentAnalyzer('/var/log/emrnext/deployment.log')
    metrics = analyzer.parse_deployment_log()
    report = analyzer.generate_deployment_report()
    print(report)

if __name__ == "__main__":
    main()
