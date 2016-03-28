@tsp = glob("*.tsp");

open ALL, ">all_tsp.pnt";
foreach $tsp(@tsp){
	open IN, "<$tsp";
		my(@line) = <IN>;
	close IN;
	
	my(@p) = ();
	foreach $line(@line){
		next unless($line =~ m/^\s*([\d\s+-.e]+)$/);
		
		my($n, $x, $y) = split /\s+/, $1;
		push @p, "{$x,$y}";
	}
	
	my($new_file) = $tsp;
	$new_file =~ s/\.tsp/.pnt/;
	$tsp =~ m/^(.+)\.tsp$/;
	open OUT, ">$new_file";
		print OUT "city={".join(",", @p)."};";
		print ALL "$1={".join(",", @p)."};\n";
	close OUT;
}
close ALL;