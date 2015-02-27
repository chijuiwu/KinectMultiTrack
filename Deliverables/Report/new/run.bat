pdflatex .\report.tex -aux-directory=.\report_aux
bibtex .\report_aux\report.aux
pdflatex .\report.tex -aux-directory=.\report_aux
pdflatex .\report.tex -aux-directory=.\report_aux

@echo off
echo.
echo.
echo.
echo Compiled report.tex. See report.pdf.
echo Chi-Jui Wu 2015
echo.