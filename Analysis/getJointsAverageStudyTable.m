function [joints_average_study_table] = getJointsAverageStudyTable(time_average_table, joints_average_types)
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
    'Kinect_Config','Scenario_Id','Person_Id'
};

% get count for people in all kinect configurations and scenarios
count_person_in_all_k_scen = 0;
for k = unique(time_average_table.Kinect_Config,'rows').'
    k_table = time_average_table(time_average_table.Kinect_Config==k,{'Scenario_Id','Person_Id'});
    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);
        count_person_in_all_k_scen = count_person_in_all_k_scen + length(unique(scen_table.Person_Id,'rows').');
    end
end

table_variable_names = [first_variable_names joints_average_types];
row_count = count_person_in_all_k_scen;
col_count = length(table_variable_names);
joints_average_study_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

filtered_variable_names = [first_variable_names joints_average_types];
first_joint_idx = length(first_variable_names)+1;

row_counter = 1;
for k = unique(time_average_table.Kinect_Config,'rows').'
    k_table = time_average_table(time_average_table.Kinect_Config==k,filtered_variable_names);

    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);

        for p_id = unique(scen_table.Person_Id,'rows').'
            person_in_k_scen = scen_table(scen_table.Person_Id==p_id,:);

            fprintf('Calculating joint averages over study - Kinect_Config=%d, Scenario=%d, Person=%d\n', ...
                k, scen_id, p_id);
            
            average_row.Kinect_Config = k;
            average_row.Scenario_Id = scen_id;
            average_row.Person_Id = p_id;
            
            for nth_joint = 1:length(joint_types)
                % 4 because Joint_dx, Joint_dy, Joint_dz, Joint_dd
                jt_idx_in_k_scen = first_joint_idx + (nth_joint-1)*4;
                % 3 because Joint_dx + 3 = Joint_dd
                person_joint = person_in_k_scen(:,jt_idx_in_k_scen:jt_idx_in_k_scen+3);
                dx=1; dy=2; dz=3; dd=4;

                avg_dx = mean(person_joint{:,dx});
                std_dx = std(person_joint{:,dx});
                avg_dy = mean(person_joint{:,dy});
                std_dy = std(person_joint{:,dy});
                avg_dz = mean(person_joint{:,dz});
                std_dz = std(person_joint{:,dz});
                avg_dd = mean(person_joint{:,dd});
                std_dd = std(person_joint{:,dd});

                % 8 because Joint_avg_dx, Joint_sd_dx, ..., Joint_sd_dd
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

            joints_average_study_table(row_counter,:) = struct2table(average_row);
            row_counter = row_counter+1;

        end
    end
end

end

