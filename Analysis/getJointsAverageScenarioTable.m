function [joints_average_scenario_table] = getJointsAverageScenarioTable(joints_average_study_table, joints_average_types)
%
% Get joints averages for each study
% 
% Scenario_Id Person_Id ...
% Joint_1_avg_dx Joint_1_sd_dx Joint_1_avg_dy Joint_1_sd_dy ...
% Joint_1_avg_dz Joint_1_sd_dz Joint_1_avg_dd Joint_1_sd_dd ...
% Joint_N_avg_dx ...
% 

joints_util;

first_variable_names = {
    'Scenario_Id','Person_Id'
};

% get count for people in all kinect configurations and scenarios
count_person_in_all_scen = 0;
for scen_id = unique(joints_average_study_table.Scenario_Id,'rows').'
    scen_table = joints_average_study_table(joints_average_study_table.Scenario_Id==scen_id,{'Kinect_Config','Person_Id'});
    if (scen_id == 1 || scen_id == 2 || scen_id == 3)
        count_person_in_all_scen = count_person_in_all_scen + length(unique(scen_table.Person_Id,'rows').');
    end
end

table_variable_names = [first_variable_names joints_average_types];
row_count = count_person_in_all_scen+1;
col_count = length(table_variable_names);
joints_average_scenario_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_idx = 4;

row_counter = 1;
for scen_id = unique(joints_average_study_table.Scenario_Id,'rows').'
    scen_table = joints_average_study_table(joints_average_study_table.Scenario_Id==scen_id,:);

    if (scen_id ~= 1 && scen_id ~= 2 && scen_id ~= 3)
        continue;
    end
    
    fprintf('Calculating joint averages over scenario - Scenario=%d\n',scen_id);
    
    for p_id = unique(scen_table.Person_Id,'rows').'
        person_in_scen = scen_table(scen_table.Person_Id==p_id,:);
        
        average_row.Scenario_Id = scen_id;
        average_row.Person_Id = p_id;
        
        jt_avg_type_idx = 1;
        for nth_joint_dx = first_joint_idx:8:(first_joint_idx-1)+length(joints_average_types)
            jt_avg_dx = mean(person_in_scen{:,nth_joint_dx});
            jt_std_dx = std(person_in_scen{:,nth_joint_dx});
            jt_avg_dy = mean(person_in_scen{:,nth_joint_dx+2});
            jt_std_dy = std(person_in_scen{:,nth_joint_dx+2});
            jt_avg_dz = mean(person_in_scen{:,nth_joint_dx+4});
            jt_std_dz = std(person_in_scen{:,nth_joint_dx+4});
            jt_avg_dd = mean(person_in_scen{:,nth_joint_dx+6});
            jt_std_dd = std(person_in_scen{:,nth_joint_dx+6});

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
        
        joints_average_scenario_table(row_counter,:) = struct2table(average_row);
        row_counter = row_counter+1;
        
    end
end

end
