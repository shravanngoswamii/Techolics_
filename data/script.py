import pdfplumber
import re
import os
from xml.etree.ElementTree import Element, SubElement, ElementTree

def capitalize_cis(text):
    return re.sub(r'\bcis\b', 'CIS', text, flags=re.IGNORECASE)

def extract_text_from_pdf(input_pdf_path):
    with pdfplumber.open(input_pdf_path) as pdf:
        text = ""
        acknowledgements_count = 0
        extracting = False

        for page in pdf.pages:
            page_text = page.extract_text()
            lines = page_text.splitlines()

            for line in lines:
                if "Acknowledgements" in line:
                    acknowledgements_count += 1
                    if acknowledgements_count == 2:
                        extracting = True
                        continue

                if extracting and "Appendix" in line:
                    return text.strip()

                if extracting:
                    text += line + "\n"

    return text.strip()

def remove_page_references(text):
    """Remove 'Page x' where x is a number from the text."""
    return re.sub(r'\bPage \d+\b', '', text)

def normalize_text(text):
    text = text.strip()
    if text.startswith("• "):
        text = text[2:]
    text = ' '.join(text.split())
    return capitalize_cis(text)

def parse_policy_data(text):
    policy_headings = list(re.finditer(r"^\d+\.\d+\.\d+.*", text, re.MULTILINE))
    policies = []

    for i in range(len(policy_headings)):
        start_pos = policy_headings[i].start()
        if i + 1 < len(policy_headings):
            end_pos = policy_headings[i + 1].start()
        else:
            end_pos = len(text)
        block_text = text[start_pos:end_pos].strip()
        policy = parse_policy_block(block_text)
        policies.append(policy)

    return policies

def parse_policy_block(block_text):
    policy = {}
    lines = block_text.strip().split('\n')
    title_line = lines[0]
    policy['id'] = title_line.split()[0]
    policy['title'] = normalize_text(title_line.strip())

    fields = ['Profile Applicability', 'Description', 'Rationale', 'Impact', 'Audit', 'Remediation', 'Default Value', 'References']
    field_pattern = r"(?<=\n)(%s):" % '|'.join([re.escape(field) for field in fields])
    field_matches = list(re.finditer(field_pattern, block_text))

    field_positions = [(m.start(), m.group(1)) for m in field_matches] + [(len(block_text), None)]

    for idx in range(len(field_positions) - 1):
        start = field_positions[idx][0]
        field_name = field_positions[idx][1]
        end = field_positions[idx + 1][0]
        content = block_text[start:end].split(":", 1)[-1].strip()
        key = field_name.lower().replace(' ', '_') if field_name else None
        if key:
            policy[key] = normalize_text(content)

    return policy

def handle_remediation(parent, value):
    split_key = "Computer"
    if split_key in value:
        description, code_part = value.split(split_key, 1)
        remediation_element = SubElement(parent, 'Remediation')
        
        if 'Note:' in code_part:
            code_part, note_part = code_part.split('Note:', 1)
            
            code_block_element = SubElement(remediation_element, 'CodeBlock')
            code_line_element = SubElement(code_block_element, 'Line')
            code_line_element.text = normalize_text("Computer " + code_part.strip())
            
            text_element = SubElement(remediation_element, 'Text')
            text_element.text = normalize_text("Note:" + note_part.strip())
        else:
            code_block_element = SubElement(remediation_element, 'CodeBlock')
            code_line_element = SubElement(code_block_element, 'Line')
            code_line_element.text = normalize_text("Computer " + code_part.strip())
        
        text_element = SubElement(remediation_element, 'Text')
        text_element.text = normalize_text(description.strip())
        
    else:
        remediation_element = SubElement(parent, 'Remediation')
        
        if 'Note:' in value:
            description_part, note_part = value.split('Note:', 1)
            text_element = SubElement(remediation_element, 'Text')
            text_element.text = normalize_text(description_part.strip())
            
            note_text_element = SubElement(remediation_element, 'Text')
            note_text_element.text = normalize_text("Note:" + note_part.strip())
        else:
            text_element = SubElement(remediation_element, 'Text')
            text_element.text = normalize_text(value)

def break_text_and_format(parent, value, delimiter="HKLM"):
    if delimiter in value:
        description, code_part = value.split(delimiter, 1)
        
        audit_element = SubElement(parent, 'Audit')
        text_element = SubElement(audit_element, 'Text')
        text_element.text = normalize_text(description.strip() + " ")
        
        code_block_element = SubElement(audit_element, 'CodeBlock')
        line_element = SubElement(code_block_element, 'Line')
        line_element.text = normalize_text(delimiter + code_part.strip())
    else:
        audit_element = SubElement(parent, 'Audit')
        text_element = SubElement(audit_element, 'Text')
        text_element.text = normalize_text(value)

def create_xml(policies):
    tag_mappings = {
        "profile_applicability": "ProfileApplicability",
        "default_value": "DefaultValue",
    }

    root = Element('Policies')
    for policy in policies:
        policy_element = SubElement(root, 'Policy', id=policy.get('id', 'unknown'))
        doc = SubElement(policy_element, 'Documentation')
        title_element = SubElement(doc, 'Title')
        title_element.text = normalize_text(policy.get('title', 'No Title'))

        for key in ['profile_applicability', 'description', 'rationale', 'impact', 'audit', 'remediation', 'default_value', 'references']:
            value = policy.get(key, '').strip()
            if value:
                if key == 'remediation':
                    handle_remediation(doc, value)
                elif key == 'references':
                    references_element = SubElement(doc, 'References')
                    joined_value = '\n'.join(normalize_text(line) for line in value.splitlines())
                    references = re.findall(r'(https?://[^\s]+)', joined_value)
                    for ref in references:
                        reference_element = SubElement(references_element, 'Reference', url=ref)
                        reference_element.text = normalize_text(ref)
                elif key == 'audit':
                    break_text_and_format(doc, value)
                else:
                    tag = tag_mappings.get(key, key.capitalize())
                    field_element = SubElement(doc, tag)
                    text_element = SubElement(field_element, 'Text')
                    text_element.text = normalize_text(value)
            else:
                tag = tag_mappings.get(key, key.capitalize())
                SubElement(doc, tag)

    return root

def save_xml_to_file(root, output_file):
    tree = ElementTree(root)
    with open(output_file, "wb") as f:
        tree.write(f, encoding='utf-8', xml_declaration=True)

if __name__ == "__main__":
    import sys
    if len(sys.argv) < 2:
        print("Error: No input PDF file specified.")
        sys.exit(1)

    input_pdf_path = sys.argv[1]  # Get the file path from the command-line arguments
    output_xml_file = r"data\policies.xml"
  # Specify your desired output XML file path

    if not os.path.exists(input_pdf_path):
        print(f"Error: The specified file does not exist: {input_pdf_path}")
        sys.exit(1)

    try:
        text = extract_text_from_pdf(input_pdf_path)
        text = remove_page_references(text)  # Remove 'Page x' references
        policies = parse_policy_data(text)
        xml_root = create_xml(policies)
        save_xml_to_file(xml_root, output_xml_file)

        print(f"XML file saved as {output_xml_file}")
    except Exception as e:
        print(f"An error occurred: {e}")
        sys.exit(1)
