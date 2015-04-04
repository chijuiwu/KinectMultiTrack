function [joints_average_kinect_config_table] = getJointsAverageKinectConfigTable(joints_average_study_table, joints_average_types)
%
% Get joints averages for each kinect config for the first three studies
% 
% Kinect_Config Person_Id ...
% Joint_1_avg_dx Joint_1_sd_dx Joint_1_avg_dy Joint_1_sd_dy ...
% Joint_1_avg_dz Joint_1_sd_dz Joint_1_avg_dd Joint_1_sd_dd ...
% Joint_N_avg_dx ...
% 

joints_util;

first_variable_names = {
    'Kinect_Config','Person_Id'
};

% get count for people in all kinect configurations for the first three
% tasks (scenarios 1, 2, and 3)
count_person_in_all_k = 0;
for k = unique(joints_average_study_table.Kinect_Config,'rows').'
    k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,{'Scenario_Id','Person_Id'});
    scen_table = k_table(k_table.Scenario_Id==1 | k_table.Scenario_Id==2 | k_table.Scenario_Id==3,:);
    count_person_in_all_k = count_person_in_all_k + length(unique(scen_table.Person_Id,'rows').');
end

table_variable_names = [first_variable_names joints_average_types];
row_count = count_person_in_all_k + 1; % plus 1 for average
col_count = length(table_variable_names);
joints_average_kinect_config_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_idx = length(first_variable_names)+1;

row_counter = 1;
for k = unique(joints_average_study_table.Kinect_Config,'rows').'
    k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,:);
    
    fprintf('Calculating joint averages over kinect config - Kinect_Config=%d\n',k);

    first_three_scenarios = k_table.Scenario_Id==1 | ...
        k_table.Scenario_Id==2 | k_table.Scenario_Id==3;
    first_three_scenarios_table = k_table(first_three_scenarios,table_variable_names);
    
    %
    % first three
    %
    average_row.Kinect_Config = k;
    average_row.Person_Id = 0; % hardcoded 1 person
    
    for nth_joint = 1:length(joint_types)
        jt_idx = first_joint_idx + (nth_joint-1)*8;
        first_three_scenario_joint = first_three_scenarios_table(:,jt_idx:jt_idx+7);
        dx=1; dy=3; dz=5; dd=7;

        avg_dx = mean(first_three_scenario_joint{:,dx});
        std_dx = std(first_three_scenario_joint{:,dx});
        avg_dy = mean(first_three_scenario_joint{:,dy});
        std_dy = std(first_three_scenario_joint{:,dy});
        avg_dz = mean(first_three_scenario_joint{:,dz});
        std_dz = std(first_three_scenario_joint{:,dz});
        avg_dd = mean(first_three_scenario_joint{:,dd});
        std_dd = std(first_three_scenario_joint{:,dd});

        % 8 because Joint_avg_dx, Joint_std_dx, ..., Joint_std_dd
        jt_avg_type_idx = 1 + (nth_joint-1)*8;

        average_row.(joints_average_types{1,jt_avg_type_idx}) = avg_dx;
        average_row.(joints_average_types{1,jt_avg_type_idx+1}) = std_dx;
        average_row.(joints_average_types{1,jt_avg_type_idx+2}) = avg_dy;
        average_row.(joints_average_types{1,jt_avg_type_idx+3}) = std_dy;
        average_row.(joints_average_types{1,jt_avg_type_idx+4}) = avg_dz;
        average_row.(joints_average_types{1,jt_avg_type_idx+5}) = std_dz;
        average_row.(joints_average_types{1,jt_avg_type_idx+6}) = avg_dd;
        average_row.(joints_average_types{1,jt_avg_type_idx+7}) = std_dd;

    end
    
    joints_average_kinect_config_table(row_counter,:) = struct2table(average_row);
    row_counter = row_counter+1;
    
end

% Average

fprintf('Calculating joint averages over kinect config - Average\n');

average_row.Kinect_Config = average_row.Kinect_Config+1;
average_row.Person_Id = 0; % hardcoded 1 person

for nth_joint = 1:length(joint_types)
    jt_idx = first_joint_idx + (nth_joint-1)*8;
    average_joint = joints_average_kinect_config_table(:,jt_idx:jt_idx+7);
    dx=1; dy=3; dz=5; dd=7;

    avg_dx = mean(average_joint{:,dx});
    std_dx = std(average_joint{:,dx});
    avg_dy = mean(average_joint{:,dy});
    std_dy = std(average_joint{:,dy});
    avg_dz = mean(average_joint{:,dz});
    std_dz = std(average_joint{:,dz});
    avg_dd = mean(average_joint{:,dd});
    std_dd = std(average_joint{:,dd});

    % 8 because Joint_avg_dx, Joint_std_dx, ..., Joint_std_dd
    jt_avg_type_idx = 1 + (nth_joint-1)*8;

    average_row.(joints_average_types{1,jt_avg_type_idx}) = avg_dx;
    average_row.(joints_average_types{1,jt_avg_type_idx+1}) = std_dx;
    average_row.(joints_average_types{1,jt_avg_type_idx+2}) = avg_dy;
    average_row.(joints_average_types{1,jt_avg_type_idx+3}) = std_dy;
    average_row.(joints_average_types{1,jt_avg_type_idx+4}) = avg_dz;
    average_row.(joints_average_types{1,jt_avg_type_idx+5}) = std_dz;
    average_row.(joints_average_types{1,jt_avg_type_idx+6}) = avg_dd;
    average_row.(joints_average_types{1,jt_avg_type_idx+7}) = std_dd;

end
    
joints_average_kinect_config_table(row_counter,:) = struct2table(average_row);

end
