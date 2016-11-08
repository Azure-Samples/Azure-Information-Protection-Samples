#pragma once

namespace Ipc
{
    using namespace System;
    using namespace System::IO;
    using namespace System::Drawing;
    using namespace System::Drawing::Printing;
    using namespace System::Windows::Forms;

    ref class IpcPrintHelper
    {
        StreamReader    ^m_streamReader;
        Font            ^m_printFont;
        PrintDocument   ^m_printDocument;
        String          ^m_currentLine;

        public:
            IpcPrintHelper(Stream ^source, Font ^font)
            {
                m_streamReader = gcnew StreamReader(source);
                m_printFont = font;
                m_printDocument = gcnew PrintDocument;
                m_currentLine = "";
            }

        protected:
            ~IpcPrintHelper()
            {
                if (m_streamReader != nullptr)
                {
                    m_streamReader->Close();
                    m_streamReader = nullptr;
                }

                m_printFont = nullptr;
                m_printDocument = nullptr;
                m_currentLine = nullptr;
            }

        public:
            //
            // Print the specified stream with the specified font
            //

            System::Void Print()
            {
                PrintDialog     ^printDialog;
                PrintDocument   ^printDocument;

                // create a print document that will handle the actual printing

                printDocument = gcnew PrintDocument;
                printDocument->PrintPage += gcnew PrintPageEventHandler(this, &IpcPrintHelper::printPage);

                // pop the print dialog with print document reference

                printDialog = gcnew PrintDialog;
                printDialog->AllowSomePages = false;
                printDialog->Document = printDocument;
                if (printDialog->ShowDialog() == System::Windows::Forms::DialogResult::OK)
                {
                    try
                    {
                        printDocument->Print();
                    }
                    catch(Exception ^)
                    {
                    }
                }
            }

        private:
            //
            // Print handler.  Printing is treated like drawing to any graphics surface.
            //

            void printPage(Object ^sender, PrintPageEventArgs ^e)
            {
                float   linesPerPage;
                float   fontHeight;
                int     countLinesPrinted;
                float   posPrintY;

                // height of the print region divided by the height of the chosen font is
                // the number of lines per page

                fontHeight = m_printFont->GetHeight(e->Graphics);
                linesPerPage = e->MarginBounds.Height / fontHeight;
                countLinesPrinted = 0;
                posPrintY = 0.0f;
                while (countLinesPrinted < linesPerPage &&
                       (m_currentLine->Length > 0 || 
                        !m_streamReader->EndOfStream))
                {
                    SizeF       ^drawnSize;
                    int         charsWritten;
                    int         linesWritten;

                    // if there's no residual text to print from the last page, then get a new line

                    if (m_currentLine->Length == 0)
                    {
                        m_currentLine = m_streamReader->ReadLine();
                    }

                    // handle wrapping for long lines by determining what the output dimensions
                    // will be and advancing starting y position for next line accordingly, and then
                    // any chars that weren't written to the page from this line will be saved
                    // for the next page iteration

                    drawnSize = e->Graphics->MeasureString(m_currentLine,
                                                           m_printFont,
                                                           SizeF((float)e->MarginBounds.Width, (float)(e->MarginBounds.Top + e->MarginBounds.Height) - posPrintY),
                                                           StringFormat::GenericDefault,
                                                           charsWritten, 
                                                           linesWritten);

                    e->Graphics->DrawString(m_currentLine,
                                            m_printFont,
                                            Brushes::Black,
                                            RectangleF((float)e->MarginBounds.Left, posPrintY, (float)e->MarginBounds.Width, (float)(e->MarginBounds.Top + e->MarginBounds.Height) - posPrintY),
                                            StringFormat::GenericDefault);

                    
                    // in the event that not all of the line could be printed, remove what was
                    // printed and save the rest for the next page

                    m_currentLine = m_currentLine->Substring(charsWritten);

                    // advance next draw Y position by the height of the drawn text

                    posPrintY += drawnSize->Height;
                    countLinesPrinted+= (int)(drawnSize->Height / fontHeight);
                }

                // at this point, either countLinesPrinted exceeds linesPerPage AND it's because
                // we couldn't write all of the last line out so have left over text to print
                // for the next page OR we're at the end of the stream and have nothing else to print

                e->HasMorePages = (m_currentLine->Length > 0 ||
                                   !m_streamReader->EndOfStream);
            }
    };
}
