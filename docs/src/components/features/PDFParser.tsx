import { Card, CardContent, CardHeader, CardTitle } from "../ui/card"
import { FileText } from "lucide-react"

export function PDFParser() {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="h-6 w-6" /> PDF Policy Parser
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <p className="text-gray-600 dark:text-gray-300">
            Advanced PDF parsing system for extracting and processing CIS benchmark policies with progress tracking and XML generation.
          </p>
          <div className="grid md:grid-cols-2 gap-4">
            <div>
              <h3 className="text-lg font-semibold mb-2">Key Features</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Intelligent text extraction using pdfplumber library</li>
                <li>Progress bar for real-time import status</li>
                <li>Structured XML output generation</li>
                <li>CIS benchmark-specific text processing</li>
                <li>Policy section detection and parsing</li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-2">Technical Details</h3>
              <ul className="list-disc pl-4 space-y-2">
                <li>Regex-based policy identification</li>
                <li>Hierarchical XML structure generation</li>
                <li>Automated field extraction (Description, Rationale, Impact)</li>
                <li>Error handling and validation</li>
                <li>Support for multiple policy formats</li>
              </ul>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
