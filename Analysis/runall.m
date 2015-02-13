filename = '../Logs/raw.csv';

valid = validateFile(filename);
if ~valid
    exit();
end

analysis(filename);