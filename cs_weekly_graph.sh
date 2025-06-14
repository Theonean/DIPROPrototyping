#!/bin/bash

# --- Configuration ---
AUTHOR_NAME="Robin"
SINCE_DATE="6 months ago"
TMP_DATA="weekly_stats.dat"
TMP_PLOT="plot.gp"

# --- Generate weekly line change data ---
git log --since="$SINCE_DATE" --author="$AUTHOR_NAME" --pretty=format:'%ad' --date=iso | \
  cut -d' ' -f1 | \
  sort | uniq -c | \
  awk '{print $2}' > dates.txt

echo "" > $TMP_DATA

while read -r date; do
  WEEK_BEGIN=$(date -d "$date" +%Y-%m-%d)
  WEEK_END=$(date -d "$date +6 days" +%Y-%m-%d)

  ADDED=0
  REMOVED=0

  while IFS= read -r line; do
    [[ $line =~ ([0-9]+)[[:space:]]insertions ]] && ADDED=${BASH_REMATCH[1]}
    [[ $line =~ ([0-9]+)[[:space:]]deletions ]] && REMOVED=${BASH_REMATCH[1]}
  done < <(git log --since="$WEEK_BEGIN" --until="$WEEK_END" --author="$AUTHOR_NAME" --shortstat --pretty=tformat: -- '*.cs')

  echo "$WEEK_BEGIN $ADDED $REMOVED" >> $TMP_DATA
done < <(git log --since="$SINCE_DATE" --author="$AUTHOR_NAME" --pretty=format:'%ad' --date=short | cut -d' ' -f1 | sort -u)

# --- Generate Gnuplot script ---
cat <<EOF > $TMP_PLOT
set xdata time
set timefmt "%Y-%m-%d"
set format x "%b-%d"
set title "C# Weekly Line Changes (Last 6 Months)"
set xlabel "Week"
set ylabel "Lines"
set key left top
set grid
plot "$TMP_DATA" using 1:2 with lines title "Added", \
     "$TMP_DATA" using 1:3 with lines title "Removed"
pause -1
EOF

# --- Plot ---
gnuplot $TMP_PLOT
