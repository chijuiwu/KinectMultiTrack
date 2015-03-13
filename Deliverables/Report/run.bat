pdflatex .\report.tex -aux-directory=.\report_aux --shell-escape
bibtex .\report_aux\report.aux
pdflatex .\report.tex -aux-directory=.\report_aux --shell-escape
pdflatex .\report.tex -aux-directory=.\report_aux --shell-escape

@echo off
echo.
echo.
echo SH Project Report (report.pdf)
echo Chi-Jui Wu 2015
echo.