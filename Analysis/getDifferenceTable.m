function [difference_table] = getDifferenceTable(raw_data_table)
%
% study kinect_config scenario_id timestamp person_id j1_dx j1_dy j1_dz j1_dd ... j25 ...
% 

unique_study = unique(raw_data_table.Study, 'rows');
unique_kinect_config = unique(raw_data_table.
unique_timestamps = unique(raw_data_table.Tracker_Time, 'rows');

difference_table = table();

display(unique_study);