function [joints_average_kinect_config_table] = getJointsAverageKinectConfigTable(joints_average_study_table, joints_average_types)
%
% Get joints averages for each study
% 
% Kinect_Config Scenario_Id Person_Id ...
% Joint_1_avg_dx Joint_1_sd_dx Joint_1_avg_dy Joint_1_sd_dy ...
% Joint_1_avg_dz Joint_1_sd_dz Joint_1_avg_dd Joint_1_sd_dd ...
% Joint_N_avg_dx ...
% 

joints_util;

first_variable_names = {
    'Kinect_Config','Person_Id'
};

% get count for people in all kinect configurations and scenarios
count_person_in_all_k = 0;
for k = unique(joints_average_study_table.Kinect_Config,'rows').'
    k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,{'Scenario_Id','Person_Id'});
    for scen_id = unique(k_table.Scenario_Id,'rows').'
        if (scen_id == 1)
            scen_table = k_table(k_table.Scenario_Id==scen_id,:);
            count_person_in_all_k = count_person_in_all_k + length(unique(scen_table.Person_Id,'rows').');
        end
    end
end

table_variable_names = [first_variable_names joints_average_types];
row_count = count_person_in_all_k;
col_count = length(table_variable_names);
joints_average_kinect_config_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_idx = 4;

row_counter = 1;
for k = unique(joints_average_study_table.Kinect_Config,'rows').'
    k_table = joints_average_study_table(joints_average_study_table.Kinect_Config==k,:);
    
    fprintf('Calculating joint averages over kinect config - Kinect_Config=%d\n',k);

    first_three_scenarios = k_table.Scenario_Id==1 | ...
        k_table.Scenario_Id==2 | k_table.Scenario_Id==3;
    first_three_scenarios_table = k_table(first_three_scenarios,:);
    
    %
    % first three
    %
    average_row.Kinect_Config = k;
    average_row.Person_Id = 0;
    jt_avg_type_idx = 1;
    for nth_joint_dx = first_joint_idx:8:(first_joint_idx-1)+length(joints_average_types)
        jt_avg_dx = mean(first_three_scenarios_table{:,nth_joint_dx});
        jt_std_dx = std(first_three_scenarios_table{:,nth_joint_dx});
        jt_avg_dy = mean(first_three_scenarios_table{:,nth_joint_dx+2});
        jt_std_dy = std(first_three_scenarios_table{:,nth_joint_dx+2});
        jt_avg_dz = mean(first_three_scenarios_table{:,nth_joint_dx+4});
        jt_std_dz = std(first_three_scenarios_table{:,nth_joint_dx+4});
        jt_avg_dd = mean(first_three_scenarios_table{:,nth_joint_dx+6});
        jt_std_dd = std(first_three_scenarios_table{:,nth_joint_dx+6});

        average_row.(joints_average_types{1,jt_avg_type_idx}) = jt_avg_dx;
        average_row.(joints_average_types{1,jt_avg_type_idx+1}) = jt_std_dx;
        average_row.(joints_average_types{1,jt_avg_type_idx+2}) = jt_avg_dy;
        average_row.(joints_average_types{1,jt_avg_type_idx+3}) = jt_std_dy;
        average_row.(joints_average_types{1,jt_avg_type_idx+4}) = jt_avg_dz;
        average_row.(joints_average_types{1,jt_avg_type_idx+5}) = jt_std_dz;
        average_row.(joints_average_types{1,jt_avg_type_idx+6}) = jt_avg_dd;
        average_row.(joints_average_types{1,jt_avg_type_idx+7}) = jt_std_dd;

        jt_avg_type_idx = jt_avg_type_idx + 8;
    end
    joints_average_kinect_config_table(row_counter,:) = struct2table(average_row);
    row_counter = row_counter+1;
    %
    % end first three
    %
end

end
