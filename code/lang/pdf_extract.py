import sys
from pypdf import PdfReader

if __name__ == "__main__":
    print(sys.argv[1])
    f = open("pdf_folder/pdf_output.txt", "w")
    reader = PdfReader(sys.argv[1])

    for i in range(len(reader.pages)):
        page = reader.pages[i]
        text = page.extract_text()
        f.write(text)

    f.close