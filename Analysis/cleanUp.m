function [skeletons_data] = cleanUp(raw_data)
%CLEANUP Clean data for post-processing
% Keep only rows where the timestamps have at least one duplicate, hence
% logs where matching skeletons was required. Tracking timestamps are 
% offset by the first recorded value.

consts;

unique_timestamps = unique(raw_data(:,log_c_tracking_timestamp), 'rows');
[count, timestamp] = histc(raw_data(:,log_c_tracking_timestamp), unique_timestamps);
multiple_entries = find(count > 1);
timestamp_multiple_entries_idx = find(ismember(timestamp, multiple_entries));
skeletons_data = raw_data(timestamp_multiple_entries_idx,:);

% offset timestamps wrt first value
init_tracking_timestamp = skeletons_data(1,log_c_tracking_timestamp);
skeletons_data(:,1) = skeletons_data(:,log_c_tracking_timestamp) - init_tracking_timestamp;

% change ms to s
skeletons_data(:,1) = skeletons_data(:,1)/1000;

% change m to c
skeletons_data(:,log_c_joint:log_c_count) = skeletons_data(:,log_c_joint:log_c_count)*100;

end
