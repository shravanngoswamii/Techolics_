import { Card, CardContent, CardHeader, CardTitle } from "../ui/card"
import { Shield } from "lucide-react"

export function PolicyManager() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Shield className="h-6 w-6" /> Policy Management System
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <p className="text-gray-600 dark:text-gray-300">
            Comprehensive policy management system supporting multiple implementation methods including Registry, Group Policy, and Secedit.
          </p>
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <h3 className="text-lg font-semibold mb-2">Core Features</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Multi-method policy configuration</li>
                <li>Automated compliance checking</li>
                <li>Value constraint validation</li>
                <li>Policy reversion support</li>
                <li>Detailed audit logging</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-2">Policy Types</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Registry-based policies</li>
                <li>Group Policy Objects</li>
                <li>Security template policies</li>
                <li>User rights assignments</li>
                <li>Password policies</li>
              </ul>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
