function [clean_data_table] = cleanData(data_table)
% 
% 
% 

joints_util;

clean_data_table = data_table;

unique_study_ids = unique(data_table.Study_Id,'rows').';
unique_kinect_configs = unique(data_table.Kinect_Config,'rows').';
unique_scenario_ids = unique(data_table.Scenario_Id,'rows').';

% 
% Find time intervals
% 
for s_id = unique_study_ids
    study_constraint = data_table.Study_Id==s_id;
    for k = unique_kinect_configs
    kinect_constraint = data_table.Kinect_Config==k;
        for scen_id = unique_scenario_ids
        scen_constraint = data_table.Scenario_Id==scen_id;
            
            rows = study_constraint & kinect_constraint & scen_constraint;
            
            time_data = data_table(rows,'Tracker_Time');
            init_time = time_data{1,'Tracker_Time'};
            
            clean_data_table{rows,'Tracker_Time'} = clean_data_table{rows,'Tracker_Time'}-init_time;
        
        end
    end
end

% 
% Convert time millisec to sec
% 
clean_data_table{:,'Tracker_Time'} = clean_data_table{:,'Tracker_Time'}/1000;

% 
% Convert coordinate meters to centimeters
% 
clean_data_table{:,coordinate_joint_types} = clean_data_table{:,coordinate_joint_types}*100;

end

