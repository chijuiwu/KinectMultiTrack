function [] = plotJoint(joint_name, joint_data)
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here
    figure;
    plot_title = strcat('Person: ',num2str(joint_data(1,2)),' Joint: ',joint_name);
    plot(joint_data(:,1), joint_data(:,5));
    title(plot_title);
    xlabel('time');
    ylabel('coordinate(m)');
end

