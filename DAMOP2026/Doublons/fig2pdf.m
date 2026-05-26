function fig2pdf(fig,name,vectorOnly)

if nargin<3
    vectorOnly=true;
end

if vectorOnly
    exportgraphics(fig, [name '.pdf'], 'BackgroundColor','none','ContentType','vector');
    % exportgraphics(fig, [name '.eps'], 'BackgroundColor','none','ContentType','vector');
    % exportgraphics(fig, [name '.eps'], 'BackgroundColor','none','ContentType','eps');
else
    % exportgraphics(fig, [name '.jpg'],'ContentType','image');
    exportgraphics(fig, [name '.jpg'], 'ContentType', 'image', 'Resolution', 600);
end

end