joint_types = {
    'AnkleL','AnkleR','ElbowL','ElbowR','FootL','FootR','HandL','HandR',...
    'HandTipL','HandTipR','Head','HipL','HipR','KneeL','KneeR','Neck',...
    'ShoulderL','ShoulderR','SpineB','SpineM','SpineS','ThumbL','ThumbR'...
    'WristL','WristR'
};
joint_count = length(joint_types);

difference_types = {'dx', 'dy', 'dz', 'dd'};
joint_difference_types = cell(1,size(joint_types,2)*size(difference_types,2));
header_count = 0;
for jt = joint_types
    for d = difference_types
        header_count = header_count+1;
        joint_difference_types(1,header_count) = strcat(jt,'_',d);
    end
end

scenario_types = {
    'Stationary'
};
scenario_count = length(scenario_types);

%
% Log
%
log_c_study_id = 1;
log_c_kinect_config = 2;
log_c_scenario = 3;
log_c_tracking_time = 4;
log_c_person = 5;
log_c_skeleton = 6;
log_c_skeleton_time = 7;
log_c_skeleton_angle = 8;
log_c_skeleton_dist = 9;
log_c_kinect = 10;
log_c_kinect_angle = 11;
log_c_kinect_height = 12;
log_c_headers = log_c_study_id:log_c_kinect_height;
log_c_joint = 13;
log_data_per_joint = 3;
log_c_x = log_c_joint;
log_c_y = log_c_joint+1;
log_c_z = log_c_joint+2;
log_c_count = length(log_c_headers) + joint_count*log_data_per_joint;

% Differences Data
%
% study_id kinect_config scenario_id timestamp person_id j1_dx j1_dy j1_dz j1_dd ... j25 ...
% 
diff_data_per_joint = 4;
diff_c_count = 2+joint_count*diff_data_per_joint;
diff_c_person = 2;
diff_c_joint = 3;

% Averages Data
%
% study_id kinect_config scenario_id person j1_dx_avg j1_dx_sd j1_dy_avg j1_dy_sd j1_dz_avg j1_dz_sd j1_dd_avg
% j1_dd_sd ... j25 ...
% 
avg_data_per_joint = 8;
avg_c_count = 1+joint_count*avg_data_per_joint;
avg_c_joint = 2;

% Single joint table
% 
% timestamp person j_dx j_dx_sd j_dy j_dj_sd j_dz j_dz_sd j_dd j_dd_sd
% 
single_data_per_joint = 8;
single_c_count = 2+single_data_per_joint;
single_c_timestamp = 1;
single_c_person = 2;
single_c_joint = 3;
% j_dx && j_dx_sd
single_c_dx = 3;
single_c_dx_sd = 4;
% j_dy && j_dy_sd
single_c_dy = 5;
single_c_dy_sd = 6;
% j_dz && j_dz_sd
single_c_dz = 7;
single_c_dz_sd = 8;
% j_dd && j_dd_sd
single_c_dd = 9;
single_c_dd_sd = 10;

% Average joint table
% 
% timestamp person dx_avg dx_sd dy_avg dy_sd dz_avg dz_sd dd_avg dd_sd
% 
average_data_per_joint = 8;
average_c_count = 2+average_data_per_joint;
average_c_timestamp = 1;
average_c_person = 2;
average_c_joint = 3;
% dx_avg && dx_sd
average_c_dx_avg = 3;
average_c_dx_sd = 4;
% dy_avg && dy_sd
average_c_dy_avg = 5;
average_c_dy_sd = 6;
% dz_avg && dz_sd
average_c_dz_avg = 7;
average_c_dz_sd = 8;
% dd_avg && dd_sd
average_c_dd_avg = 9;
average_c_dd_sd = 10;

% All joint table
% 
% person joint j_dx_avg j_dx_sd j_dy_avg j_dy_sd j_dz_avg j_dz_sd j_dd_avg j_dd_sd
% 
all_data_per_joint = 8;
all_c_count = 2+all_data_per_joint;
all_c_person = 1;
all_c_jointtype = 2;
all_c_joint = 3;
% j_dx_avg && j_dx_sd
all_c_dx_avg = 3;
all_c_dx_sd = 4;
% j_dy_avg && j_dy_sd
all_c_dy_avg = 5;
all_c_dy_sd = 6;
% j_dz_avg && j_dz_sd
all_c_dz_avg = 7;
all_c_dz_sd = 8;
% j_dd_avg && j_dd_sd
all_c_dd_avg = 9;
all_c_dd_sd = 10;

% All scenarios table
% 
% scenario person time_dependent_dd_avg time_dependent_dd_sd
% 
scenarios_data_per_joint = 2;
scenarios_c_count = 2+scenarios_data_per_joint;
scenarios_c_scenario = 1;
scenarios_c_person = 2;
scenarios_c_joint = 3;
scenarios_c_dd_avg = 3;
scenarios_c_dd_sd = 4;