function [valid] = validateFile(filename)
%VALIDATEFILE Summary of this function goes here
%   Detailed explanation goes here

consts

file = fopen(filename);
header_line = fgetl(file);
headers = strsplit(header_line, ', ');
if size(headers, 2) ~= (log_c_count)
    display('File format not correct');
    valid = false;
end
fclose(file);

valid = true;

end

