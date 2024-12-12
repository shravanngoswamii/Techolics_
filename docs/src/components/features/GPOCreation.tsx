import { Card, CardContent, CardHeader, CardTitle } from "../ui/card"
import { Settings } from "lucide-react"

export function GPOCreation() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Settings className="h-6 w-6" /> Group Policy Management
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <p className="text-gray-600 dark:text-gray-300">
            Comprehensive Group Policy Object (GPO) management system with PowerShell integration and automated policy configuration.
          </p>
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <h3 className="text-lg font-semibold mb-2">Features</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>PowerShell-based GPO configuration</li>
                <li>Automated policy value detection</li>
                <li>Default value management</li>
                <li>Domain vs. Standalone handling</li>
                <li>Detailed logging system</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-2">Implementation</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Get-GPRegistryValue integration</li>
                <li>Set-GPRegistryValue automation</li>
                <li>Value constraint validation</li>
                <li>Error handling and recovery</li>
                <li>Policy reversion support</li>
              </ul>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
