function [table] = readData(dir_name, table)
% 
% 
% 

file_names = dir(strcat(dir_name,'*.csv'));
for i=1:length(file_names)
    file_name = strcat(dir_name,file_names(i).name);
    fprintf('Loading file, filename=%s\n',file_name);
    table = [table;readtable(file_name,'ReadVariableNames',true)];
end

end

