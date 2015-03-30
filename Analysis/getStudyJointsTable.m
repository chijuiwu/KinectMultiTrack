function [study_joints_table] = getStudyJointsTable(time_average_table, time_average_joint_types)
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

% get row count
total_row_count = 0;
for k = unique(time_average_table.Kinect_Config,'rows').'
    k_table = time_average_table(time_average_table.Kinect_Config==k,{'Scenario_Id'});
    total_row_count = total_row_count + length(unique(k_table.Scenario_Id,'rows').');
end

table_variable_names = [first_variable_names time_average_joint_types];
row_count = total_row_count;
col_count = length(table_variable_names);
study_joints_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

filtered_variable_names = [first_variable_names time_average_joint_types];
first_joint_idx = length(first_variable_names)+1;

row_counter = 1;
for k = unique(time_average_table.Kinect_Config,'rows').'
    k_table = time_average_table(time_average_table.Kinect_Config==k,filtered_variable_names);

    for scen_id = unique(k_table.Scenario_Id,'rows').'
        scen_table = k_table(k_table.Scenario_Id==scen_id,:);

        for p_id = unique(scen_table.Person_Id,'rows').'
            person_joints_data = scen_table(scen_table.Person_Id==p_id,:);

            average_row.Kinect_Config = k;
            average_row.Scenario_Id = scen_id;
            average_row.Person_Id = p_id;
            
            for jt_num = 1:length(joint_types)
                % 4 because Joint_dx, Joint_dy, Joint_dz, Joint_dd
                jt_idx = first_joint_idx + (jt_num-1)*4;
                % 3 because Joint_dx + 3 = Joint_dd (indices)
                study_joints_data = person_joints_data(:,jt_idx:jt_idx+3);
                dx=1; dy=2; dz=3; dd=4;

                avg_dx = mean(study_joints_data{:,dx});
                std_dx = std(study_joints_data{:,dx});
                avg_dy = mean(study_joints_data{:,dy});
                std_dy = std(study_joints_data{:,dy});
                avg_dz = mean(study_joints_data{:,dz});
                std_dz = std(study_joints_data{:,dz});
                avg_dd = mean(study_joints_data{:,dd});
                std_dd = std(study_joints_data{:,dd});

                % 8 because Joint_avg_dx, Joint_sd_dx, ..., Joint_sd_dd
                jt_avg_type_idx = 1 + (jt_num-1)*8;

                average_row.(time_average_joint_types{1,jt_avg_type_idx}) = avg_dx;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+1}) = std_dx;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+2}) = avg_dy;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+3}) = std_dy;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+4}) = avg_dz;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+5}) = std_dz;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+6}) = avg_dd;
                average_row.(time_average_joint_types{1,jt_avg_type_idx+7}) = std_dd;
            end

            study_joints_table(row_counter,:) = struct2table(average_row);
            row_counter = row_counter+1;

        end
    end
end

end

