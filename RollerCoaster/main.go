package main

import (
	"fmt"
	"os"
	"time"
)

type sum struct {
	people int
	offset int
}

func main() {
	var places, rides, groupsNumber int
	fmt.Scan(&places, &rides, &groupsNumber)

	d("places:", places, "rides:", rides, "groupsNumber:", groupsNumber)

	groups := make([]int, groupsNumber)
	knownSets := make(map[int]sum)

	for i := 0; i < groupsNumber; i++ {
		var Pi int
		fmt.Scan(&Pi)
		groups[i] = Pi
	}

	var dirhams uint64
	var people int
	var groupsLoaded int

	t0 := time.Now()
	n := 0
	for i := 0; i < rides; i++ {
		people = 0
		groupsLoaded = 0

		knownSet, ok := knownSets[n%groupsNumber]
		if ok {
			people = knownSet.people
			n = knownSet.offset
		} else {
			startOffset := n % groupsNumber
			for people+groups[n%groupsNumber] <= places && groupsLoaded < groupsNumber {
				people += groups[n%groupsNumber]
				groupsLoaded++
				n++
			}
			knownSets[startOffset] = sum{people, n % groupsNumber}
		}
		dirhams += uint64(people)
	}

	t1 := time.Now()
	d("The call took to run:", t1.Sub(t0))

	fmt.Println(dirhams)
}

func d(a ...interface{}) {
	fmt.Fprintln(os.Stderr, a)
}
