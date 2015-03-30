function [time_average_table, time_average_joint_types] = getTimeAverageTable(difference_table, difference_joint_types)
%
% Study_Id Kinect_Config Scenario_Id Person_Id ...
% Joint_1_avg_dx Joint_1_sd_dx Joint_1_avg_dy Joint_1_sd_dy ...
% Joint_1_avg_dz Joint_1_sd_dz Joint_1_avg_dd Joint_1_sd_dd ...
% Joint_N_avg_dx ...
% 

joints_util;

first_variable_names = {
    'Study_Id','Kinect_Config','Scenario_Id','Person_Id'
};

% 
% AnkleLeft_avg_dx, AnkleLeft_sd_dx, AnkleLeft_avg_dy, AnkleLeft_sd_dy, ...
% AnkleLeft_avg_dz, AnkleLeft_sd_dz, AnkleLeft_avg_dd, AnkleLeft_sd_dd, ...
% 
average_types = {
    'avg_dx','sd_dx','avg_dy','sd_dy','avg_dz','sd_dz','avg_dd','sd_dd'
};
time_average_joint_types = cell(1,length(joint_types)*length(average_types));
counter = 0;
for jt = joint_types
    for a = average_types
        counter = counter+1;
        time_average_joint_types(1,counter) = strcat(jt,'_',a);
    end
end

filtered_variable_names = [first_variable_names difference_joint_types];

% get row count
total_row_count = 0;
for s_id = unique(difference_table.Study_Id,'rows').'
    s_table = difference_table(difference_table.Study_Id==s_id,filtered_variable_names);
    
    for k = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==k,:);
        
        for scen_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scen_id,:);
            
            total_row_count = total_row_count+length(unique(scen_table.Person_Id,'rows').');
        end
    end
end

table_variable_names = [first_variable_names time_average_joint_types];
row_count = total_row_count;
col_count = length(table_variable_names);
time_average_table = array2table(zeros(row_count,col_count),'VariableNames',table_variable_names);
average_row = struct();
for field = table_variable_names
    average_row.(char(field)) = 0;
end

first_joint_idx = length(first_variable_names)+1;

row_counter = 1;
for s_id = unique(difference_table.Study_Id,'rows').'
    s_table = difference_table(difference_table.Study_Id==s_id,filtered_variable_names);
    
    for k = unique(s_table.Kinect_Config,'rows').'
        k_table = s_table(s_table.Kinect_Config==k,:);
        
        for scen_id = unique(k_table.Scenario_Id,'rows').'
            scen_table = k_table(k_table.Scenario_Id==scen_id,:);
            
            for p_id = unique(scen_table.Person_Id,'rows').'
                person_joints_data = scen_table(scen_table.Person_Id==p_id,:);
                
                average_row.Study_Id = s_id;
                average_row.Kinect_Config = k;
                average_row.Scenario_Id = scen_id;
                average_row.Person_Id = p_id;
                
                for jt_num = 1:length(joint_types)
                    % 4 because Joint_dx, Joint_dy, Joint_dz, Joint_dd
                    jt_idx = first_joint_idx + (jt_num-1)*4;
                    % 3 because Joint_dx + 3 = Joint_dd (indices)
                    joints_data_over_time = person_joints_data(:,jt_idx:jt_idx+3);
                    dx=1; dy=2; dz=3; dd=4;
                    
                    avg_dx = mean(joints_data_over_time{:,dx});
                    std_dx = std(joints_data_over_time{:,dx});
                    avg_dy = mean(joints_data_over_time{:,dy});
                    std_dy = std(joints_data_over_time{:,dy});
                    avg_dz = mean(joints_data_over_time{:,dz});
                    std_dz = std(joints_data_over_time{:,dz});
                    avg_dd = mean(joints_data_over_time{:,dd});
                    std_dd = std(joints_data_over_time{:,dd});
                    
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
        
                time_average_table(row_counter,:) = struct2table(average_row);
                row_counter = row_counter+1;
        
            end
        end
    end
end
