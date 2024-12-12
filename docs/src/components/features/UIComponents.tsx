import { Card, CardContent, CardHeader, CardTitle } from "../ui/card"
import { Layout } from "lucide-react"

export function UIComponents() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Layout className="h-6 w-6" /> Modern UI Framework
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <p className="text-gray-600 dark:text-gray-300">
            Modern and responsive user interface built with WPF and FluentUI framework, providing a seamless Windows-native experience.
          </p>
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <h3 className="text-lg font-semibold mb-2">UI Components</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>FluentWindow base implementation</li>
                <li>Policy editor dialogs</li>
                <li>Dynamic value input controls</li>
                <li>Progress indicators</li>
                <li>Status visualization</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-2">Features</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Responsive grid layouts</li>
                <li>Type-specific input validation</li>
                <li>User/machine policy separation</li>
                <li>Intuitive navigation system</li>
                <li>Modern styling and animations</li>
              </ul>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
