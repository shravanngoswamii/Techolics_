import React from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Sun, Moon } from "lucide-react"
import { Logo } from "./components/Logo"
import { useTheme } from "./components/theme-provider"

const ThemeToggle = () => {
  const { theme, setTheme } = useTheme()
  return (
    <button onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')} className="p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-800">
      {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
    </button>
  )
}

const Navigation = () => (
  <nav className="flex items-center justify-between p-4 border-b dark:border-gray-800">
    <div className="flex items-center space-x-4">
      <Logo />
      <h2 className="text-xl font-semibold text-gray-900 dark:text-white">Documentation</h2>
    </div>
    <ThemeToggle />
  </nav>
)

interface TabContentProps {
  title: string
  description: string
  children?: React.ReactNode
}

const TabContent: React.FC<TabContentProps> = ({ title, description, children }) => (
  <Card>
    <CardHeader>
      <CardTitle>{title}</CardTitle>
      <CardDescription>{description}</CardDescription>
    </CardHeader>
    <CardContent>
      <div className="space-y-4">
        {children}
      </div>
    </CardContent>
  </Card>
)

function App() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-4 md:p-8">
      <header className="max-w-6xl mx-auto">
        <Navigation />
        <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2">Techolics</h1>
        <p className="text-lg text-gray-600 dark:text-gray-300 mb-8">
          Windows Policy Management and Security Auditing Application
        </p>
      </header>

      <main className="max-w-6xl mx-auto">
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-2">
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="policy">Policy Management</TabsTrigger>
            <TabsTrigger value="security">Security</TabsTrigger>
            <TabsTrigger value="registry">Registry</TabsTrigger>
            <TabsTrigger value="ui">UI Components</TabsTrigger>
            <TabsTrigger value="logging">Logging</TabsTrigger>
            <TabsTrigger value="architecture">Architecture</TabsTrigger>
          </TabsList>

          <TabsContent value="overview">
            <TabContent
              title="Project Overview"
              description="A comprehensive Windows policy management and security auditing solution"
            >
              <div className="grid md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <h3 className="font-semibold">Key Features</h3>
                  <ul className="list-disc pl-4 space-y-1">
                    <li>Policy Configuration and Auditing</li>
                    <li>PDF Policy Import and Parsing</li>
                    <li>GPO Creation and Management</li>
                    <li>Registry Management</li>
                    <li>Security Policy Management</li>
                    <li>Modern WPF UI with FluentUI</li>
                    <li>Comprehensive Logging</li>
                  </ul>
                </div>
                <div className="space-y-2">
                  <h3 className="font-semibold">Technologies</h3>
                  <ul className="list-disc pl-4 space-y-1">
                    <li>C# / .NET</li>
                    <li>Windows Registry API</li>
                    <li>Group Policy API</li>
                    <li>Secedit Utility</li>
                    <li>WPF with FluentUI</li>
                    <li>XAML UI Framework</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="policy">
            <TabContent
              title="Policy Management"
              description="Configure and manage Windows policies with ease"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">PDF Policy Import</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Import and parse policy configurations from PDF documents with real-time progress tracking. The system uses advanced PDF parsing to extract policy definitions, values, and documentation.
                  </p>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">GPO Creation</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Create and manage Group Policy Objects with custom names, descriptions, and automated folder structures. Supports bulk policy application and template-based configurations.
                  </p>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">Policy Configuration</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Comprehensive policy management through the PolicyConfigurator class, supporting multiple implementation methods including Registry, Group Policy, and Secedit.
                  </p>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="security">
            <TabContent
              title="Security Features"
              description="Advanced security auditing and compliance tools"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">Security Policy Management</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Comprehensive security policy management through SeceditManager, providing:
                  </p>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Windows security configuration management</li>
                    <li>Security template processing</li>
                    <li>Policy compliance verification</li>
                    <li>Automated security baseline enforcement</li>
                  </ul>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">New Policy Implementations</h3>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Enhanced password policies with complexity requirements</li>
                    <li>Account lockout policy management</li>
                    <li>User rights assignment policies</li>
                    <li>Security options configuration</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="registry">
            <TabContent
              title="Registry Management"
              description="Windows Registry configuration and monitoring"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">Registry Operations</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    The RegistryManager provides comprehensive Windows Registry management:
                  </p>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Key creation and modification</li>
                    <li>Value management (String, DWORD, QWORD)</li>
                    <li>Security descriptor handling</li>
                    <li>Registry permission management</li>
                  </ul>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">Policy Implementation</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Registry-based policy configurations supporting:
                  </p>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>System policies</li>
                    <li>User preferences</li>
                    <li>Application settings</li>
                    <li>Security configurations</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="ui">
            <TabContent
              title="UI Components"
              description="WPF-based user interface components"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">WPF Framework Integration</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Modern UI built with Windows Presentation Foundation (WPF) and enhanced with FluentUI design system. Features include:
                  </p>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Responsive grid layouts with XAML</li>
                    <li>Custom WPF controls and templates</li>
                    <li>FluentUI design components</li>
                    <li>Dark/Light theme support</li>
                    <li>Animated transitions and effects</li>
                  </ul>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">Key Windows</h3>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>MainWindow - Primary application interface</li>
                    <li>PolicyExplorerWindow - Policy management interface</li>
                    <li>EditPolicyWindow - Policy modification dialog</li>
                    <li>SplashScreen - Animated loading screen</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="logging">
            <TabContent
              title="Logging System"
              description="Comprehensive activity logging and monitoring"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">Logging Features</h3>
                  <p className="text-gray-600 dark:text-gray-300 mb-4">
                    Enhanced logging system with detailed event tracking:
                  </p>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Policy changes and modifications</li>
                    <li>Security audit events</li>
                    <li>User actions and system events</li>
                    <li>Error and exception handling</li>
                  </ul>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">Implementation Details</h3>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Singleton Logger pattern for consistent logging</li>
                    <li>Structured log format with timestamps</li>
                    <li>Log rotation and archival</li>
                    <li>Log level management (Debug, Info, Warning, Error)</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>

          <TabsContent value="architecture">
            <TabContent
              title="System Architecture"
              description="Technical architecture and component relationships"
            >
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-semibold mb-2">Core Components</h3>
                  <div className="grid md:grid-cols-2 gap-4">
                    <div>
                      <h4 className="font-medium mb-2">Policy Management</h4>
                      <ul className="list-disc pl-4 space-y-1">
                        <li>PolicyConfigurator</li>
                        <li>PolicyAuditor</li>
                        <li>PolicyValueConverter</li>
                      </ul>
                    </div>
                    <div>
                      <h4 className="font-medium mb-2">System Integration</h4>
                      <ul className="list-disc pl-4 space-y-1">
                        <li>RegistryManager</li>
                        <li>GroupPolicyManager</li>
                        <li>SeceditManager</li>
                      </ul>
                    </div>
                  </div>
                </div>
                <div>
                  <h3 className="text-lg font-semibold mb-2">Project Structure</h3>
                  <ul className="list-disc pl-4 space-y-2">
                    <li>Models/ - Data models for CIS Benchmarks</li>
                    <li>PolicyManagement/ - Core policy management classes</li>
                    <li>Logging/ - Logging functionality</li>
                    <li>pages/ - UI-specific windows and components</li>
                    <li>data/ - Configuration and benchmark data files</li>
                  </ul>
                </div>
              </div>
            </TabContent>
          </TabsContent>
        </Tabs>
      </main>
    </div>
  )
}

export default App
