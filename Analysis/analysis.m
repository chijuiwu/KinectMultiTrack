function [] = analysis(raw_filename)

idx_timestamp = 1;
idx_person = 2;
idx_skeleton = 3;
idx_fov = 4;
total_skeletons = 25

raw_file = fopen(raw_filename);
header_line = fgetl(raw_file);
headers = strsplit(header_line, ', ');
if size(headers, 2) ~= (total_skeletons*3+4)
    display('File format not correct...Exit');
    exit();
end
fclose(raw_file);

raw_data = csvread(raw_filename, 1);
unique_timestamps = unique(raw_data(:,idx_timestamp), 'rows');
[count, timestamp] = histc(raw_data(:,idx_timestamp), unique_timestamps);
multiple_entries = find(count > 1);
idx_timestamp_multiple_entries = find(ismember(timestamp, multiple_entries));

skeletons_data = raw_data(idx_timestamp_multiple_entries,:);
% unique_timestamps = unique(skeletons_data(:,idx_timestamp), 'rows');
% unique_person_ids = unique(skeletons_data(:,idx_person), 'rows');

joint = 

format long;
display(unique_person_ids);

end