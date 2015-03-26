function [] = plotAllJoints(all_joints_table)
consts;

person_id = all_joints_table(1,all_c_person);
plot_title = sprintf('Person %d All Joints',person_id);
plot_filename = sprintf('Plots/Person_%d_AllJoints',person_id);

x = all_joints_table(:,all_c_jointtype);
dx_avg = all_joints_table(:,all_c_dx_avg);
dx_sd = all_joints_table(:,all_c_dx_sd);
dy_avg = all_joints_table(:,all_c_dy_avg);
dy_sd = all_joints_table(:,all_c_dy_sd);
dz_avg = all_joints_table(:,all_c_dz_avg);
dz_sd = all_joints_table(:,all_c_dz_sd);
dd_avg = all_joints_table(:,all_c_dd_avg);
dd_sd = all_joints_table(:,all_c_dd_sd);

figure;
hold on;
errorbar(x,dx_avg,dx_sd,'-r','LineStyle','none','Marker','x');
errorbar(x,dy_avg,dy_sd,'-g','LineStyle','none','Marker','x');
errorbar(x,dz_avg,dz_sd,'-b','LineStyle','none','Marker','x');
errorbar(x,dd_avg,dd_sd,'-k','LineStyle','none','Marker','o');
set(gca,'XLim',[0.5 joint_count+0.5])
set(gca,'XTick',1:joint_count,'XTickLabel',joint_types);
rotateticklabel(gca, -90);
hold off;

title(plot_title);
x_axis_label = xlabel('Joints');
set(x_axis_label,'Position',get(x_axis_label,'Position')-[0,4,0]);
ylabel('Distance(cm)');
legend('\Delta x','\Delta y','\Delta z','\Delta d','Location','northeastoutside');

set(gcf, 'PaperPositionMode', 'manual');
set(gcf, 'PaperUnits', 'normalized');
set(gcf, 'PaperPosition', [0 0 1 0.6])
print('-dsvg', '-painters', plot_filename);

end