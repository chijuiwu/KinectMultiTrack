function [] = analysis(raw_filename)
consts;

raw_file = fopen(raw_filename);
header_line = fgetl(raw_file);
headers = strsplit(header_line, ', ');
if size(headers, 2) ~= (headers_count)
    display('File format not correct...Exit');
    exit();
end
fclose(raw_file);

raw_data = csvread(raw_filename, 1);
unique_timestamps = unique(raw_data(:,col_timestamp), 'rows');
[count, timestamp] = histc(raw_data(:,col_timestamp), unique_timestamps);
multiple_entries = find(count > 1);
idx_timestamp_multiple_entries = find(ismember(timestamp, multiple_entries));

skeletons_data = raw_data(idx_timestamp_multiple_entries,:);
unique_timestamps = unique(skeletons_data(:,col_timestamp), 'rows');
unique_person_ids = unique(skeletons_data(:,col_person), 'rows');

% Clean up timestamps
first_timestamp = unique_timestamps(1);
skeletons_data(:,1) = skeletons_data(:,1) - first_timestamp;

% For each person
for person_idx = 1:numel(unique_person_ids)
    person_id = unique_person_ids(person_idx);
    person_data = skeletons_data(skeletons_data(:,col_person)==person_id,:);
    
    % For each joint
    for joint_header_idx = col_joint:3:(headers_count)
        joint_header = headers{joint_header_idx};
        joint_name = joint_header(1:length(joint_header)-2);
        joint_data = person_data(:,[col_headers, joint_header_idx:joint_header_idx+2]);
        
        % avg
        avg_x = mean(joint_data(:,col_x));
        avg_y = mean(joint_data(:,col_y));
        avg_z = mean(joint_data(:,col_z));
        
        single_joint = [];
        % For each timestamp
        for timestamp_idx = 1:numel(unique_timestamps)
            timestamp = unique_timestamps(timestamp_idx);
            timestamp_data = joint_data(joint_data(:,col_timestamp)==timestamp,:);
            d_x = abs(timestamp_data(1,col_x) - timestamp_data(2,col_x));
            d_y = abs(timestamp_data(1,col_y) - timestamp_data(2,col_y));
            d_z = abs(timestamp_data(1,col_z) - timestamp_data(2,col_z));
            d_d = sqrt(d_x^2 + d_y^2 + d_z^2);
            instance = [d_x, d_y, d_z, d_d];
            single_joint = [single_joint;instance];
        end
        plotJoint(joint_name, single_joint);
    end
    
    
end
% For each joint

end