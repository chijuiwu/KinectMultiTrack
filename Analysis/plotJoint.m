function [] = plotJoint(joint_name, single_joint_table)
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here
consts;

person_id = single_joint_table(1,single_c_person);
plot_title = sprintf('Person %d Joint %s',person_id,joint_name);
plot_filename = sprintf('Plots/Person_%d_Joint_%s',person_id,joint_name);

x = single_joint_table(:,single_c_timestamp);
dist_dx = single_joint_table(:,single_c_dx);
dx_sd = single_joint_table(:,single_c_dx_sd);
dist_dy = single_joint_table(:,single_c_dy);
dy_sd = single_joint_table(:,single_c_dy_sd);
dist_dz = single_joint_table(:,single_c_dz);
dz_sd = single_joint_table(:,single_c_dz_sd);
dist_dd = single_joint_table(:,single_c_dd);
dd_sd = single_joint_table(:,single_c_dd_sd);

figure;
hold on;
shadedErrorBar(x,dist_dx,dx_sd,'-r', 1);
shadedErrorBar(x,dist_dy,dy_sd,'-g', 1);
shadedErrorBar(x,dist_dz,dz_sd,'-b', 1);
shadedErrorBar(x,dist_dd,dd_sd,'-k', 1);
xlim([0 30]);
set(gcf,'visible','off')
hold off;

title(plot_title);
xlabel('Time(s)');
ylabel('Distance(m)');
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northeastoutside');
print('-dpdf', '-painters', plot_filename);
end
