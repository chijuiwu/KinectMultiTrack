function [] = analysis(raw_filename)
consts;

raw_data = csvread(raw_filename, 1);
skeletons_data = cleanUp(raw_data);

unique_timestamps = unique(skeletons_data(:,log_c_tracking_timestamp), 'rows');
unique_timestamps_count = size(unique_timestamps, 1);
unique_persons = unique(skeletons_data(:,log_c_person), 'rows');
unique_persons_count = size(unique_persons, 1);

% Differences data
% 
% @see consts#Differences Data
% 
diff_data = zeros(unique_timestamps_count*unique_persons_count, diff_c_count);
% For each timestamp
for row_idx = 1:numel(unique_timestamps)
    timestamp = unique_timestamps(row_idx);
    diff_at_t = zeros(1, diff_c_count);
    diff_at_t(1) = timestamp;
    selected_t = skeletons_data(skeletons_data(:,log_c_tracking_timestamp)==timestamp,:);
    
    % For each person
    for person_idx = 1:numel(unique_persons)
        person_id = unique_persons(person_idx);
        diff_at_t(2) = person_id;
        selected_p = selected_t(selected_t(:,log_c_person)==person_id,:);    
        
        % For each joint each
        for joint_idx = 1:joint_count
            log_joint_idx = log_c_joint+(joint_idx-1)*log_data_per_joint;
            selected_j = selected_p(:,log_joint_idx:log_joint_idx+log_data_per_joint-1);
            
            % Hack: Assume 2 skeletons             
            dx = abs(selected_j(1,1)-selected_j(2,1));
            dy = abs(selected_j(1,2)-selected_j(2,2));
            dz = abs(selected_j(1,3)-selected_j(2,3));
            dd = sqrt(dx^2 + dy^2 + dz^2);
            
            diff_data_idx = diff_c_joint+(joint_idx-1)*diff_data_per_joint;
            diff_at_t(diff_data_idx:diff_data_idx+diff_data_per_joint-1) = [dx dy dz dd];
        end
        diff_data(row_idx,:) = diff_at_t;
    end
end


% Averages data
% 
% @see consts#Averages Data
% 
avg_data = zeros(unique_persons_count,avg_c_count);
% For each person
for row_idx = 1:numel(unique_persons)
    person_id = unique_persons(row_idx);
    selected_p = diff_data(diff_data(:,diff_c_person)==person_id,:);    
    avg_at_p = zeros(1, avg_c_count);
    avg_at_p(1) = person_id;

    % For each joint
    for joint_idx = 1:joint_count
        diff_joint_idx = diff_c_joint+(joint_idx-1)*diff_data_per_joint;
        selected_j = selected_p(:,diff_joint_idx:diff_joint_idx+diff_data_per_joint-1);

        % Hack: Assume 2 skeletons             
        avg_dx = mean(selected_j(:,1));
        sd_dx = std(selected_j(:,1));
        avg_dy = mean(selected_j(:,2));
        sd_dy = std(selected_j(:,2));
        avg_dz = mean(selected_j(:,3));
        sd_dz = std(selected_j(:,3));
        avg_dd = mean(selected_j(:,4));
        sd_dd = std(selected_j(:,4));
        
        avg_data_idx = avg_c_joint+(joint_idx-1)*avg_data_per_joint;
        avg_at_p(avg_data_idx:avg_data_idx+avg_data_per_joint-1) = [avg_dx sd_dx avg_dy sd_dy avg_dz sd_dz avg_dd sd_dd];
    end
    avg_data(row_idx,:) = avg_at_p;
end

% Single joint plot
% 
% @see consts#Single joint table
% Hack: For 1 person
% 
for joint_idx = 1:joint_count
    row_count = unique_timestamps_count;
    single_joint_table = zeros(row_count,single_c_count);
    joint_name = joint_types{joint_idx};
    
    diff_joint_idx = diff_c_joint+(joint_idx-1)*diff_data_per_joint;
    avg_joint_idx = avg_c_joint+(joint_idx-1)*avg_data_per_joint;
    
    % timestamp, person     
    single_joint_table(:,1:2) = diff_data(:,1:2);
    % dx && dx_sd 
    single_joint_table(:,3) = diff_data(:,diff_joint_idx); 
    single_joint_table(:,4) = ones(row_count,1)*avg_data(1,avg_joint_idx+1);
    % dy && dy_sd
    single_joint_table(:,5) = diff_data(:,diff_joint_idx+1);
    single_joint_table(:,6) = ones(row_count,1)*avg_data(1,avg_joint_idx+3);
    % dz && dz_sd
    single_joint_table(:,7) = diff_data(:,diff_joint_idx+2);
    single_joint_table(:,8) = ones(row_count,1)*avg_data(1,avg_joint_idx+5);
    % dd && dd_sd
    single_joint_table(:,9) = diff_data(:,diff_joint_idx+3);
    single_joint_table(:,10) = ones(row_count,1)*avg_data(1,avg_joint_idx+7);
    % plot    
    plotJoint(joint_name, single_joint_table);
end

% Average joint plot
% 
% @see consts#Average joint table
% Hack: For 1 person
% 
average_joint_table = zeros(unique_timestamps_count,average_c_count);
average_joint_table(:,1:2) = diff_data(:,1:2);
% dx
all_dx = diff_data(:,diff_c_joint:diff_data_per_joint:diff_c_count);
average_joint_table(:,3) = mean(all_dx, 2);
average_joint_table(:,4) = std(all_dx, 0, 2);
% dy
all_dy = diff_data(:,diff_c_joint+1:diff_data_per_joint:diff_c_count);
average_joint_table(:,5) = mean(all_dy, 2);
average_joint_table(:,6) = std(all_dy, 0, 2);
% dz
all_dz = diff_data(:,diff_c_joint+2:diff_data_per_joint:diff_c_count);
average_joint_table(:,7) = mean(all_dz, 2);
average_joint_table(:,8) = std(all_dz, 0, 2);
% dd
all_dd = diff_data(:,diff_c_joint+3:diff_data_per_joint:diff_c_count);
average_joint_table(:,9) = mean(all_dd, 2);
average_joint_table(:,10) = std(all_dd, 0, 2);
% plot
plotAvgJoint(average_joint_table);

% All joint plot
% 
% @see consts#All joint table
% Hack: For 1 person
% 
all_joints_table = zeros(joint_count, all_c_count);
all_joints_table(:,1:2) = [zeros(joint_count,1) transpose(1:joint_count)];
for joint_idx = 1:joint_count
    avg_joint_idx = avg_c_joint + (joint_idx-1)*avg_data_per_joint;
    sd_joint_idx = average_c_joint;
    % dx
    all_joints_table(joint_idx,3) = avg_data(1,avg_joint_idx);
    % dy
    all_joints_table(joint_idx,5) = avg_data(1,avg_joint_idx+2);
    % dz
    all_joints_table(joint_idx,7) = avg_data(1,avg_joint_idx+4);
    % dd
    all_joints_table(joint_idx,9) = avg_data(1,avg_joint_idx+6);
end
all_joints_table(:,4) = std(all_joints_table(:,3));
all_joints_table(:,6) = std(all_joints_table(:,5));
all_joints_table(:,8) = std(all_joints_table(:,7));
all_joints_table(:,10) = std(all_joints_table(:,9));
% plot
plotAllJoints(all_joints_table);

% All scenarios
% 
% @see consts#All scenarios table
% Hack: For 1 scenario 1 person
% 
all_scenarios_table = zeros(1,scenarios_c_count);
all_scenarios_table(1,1:2) = [0 0];
time_dependent_dd_avg = mean(all_joints_table(:,all_c_dd_avg));
time_dependent_dd_sd = std(all_joints_table(:,all_c_dd_avg));
all_scenarios_table(1,3:4) = [time_dependent_dd_avg time_dependent_dd_sd];
% plot
plotAllScenarios(all_scenarios_table);

end