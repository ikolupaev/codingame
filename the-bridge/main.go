package main

import (
	"fmt"
	"io"
	"os"
)

type bike struct {
	x, y, active int
}

var moves = []string{"SPEED", "JUMP", "SLOW", "WAIT", "UP", "DOWN"}

var tracks []string
var bikesNumber int
var minBikes int

var dbg io.Writer

func d(a ...interface{}) {
	//fmt.Fprintln(dbg, a)
}

func main() {
	run(os.Stdin, os.Stdout, os.Stderr)
}

func run(in io.Reader, out io.Writer, debug io.Writer) {
	dbg = debug

	readInitData(in)

	for {
		bikes, speed := readStepData(in)
		move := findBestMove(bikes, speed, 0)
		fmt.Fprintln(out, move)
	}
}

func readInitData(in io.Reader) {
	fmt.Fscan(in, &bikesNumber)
	fmt.Fscan(in, &minBikes)
	d(bikesNumber)
	d(minBikes)

	tracks = make([]string, 4)

	for i := 0; i < 4; i++ {
		fmt.Fscan(in, &tracks[i])
		d(tracks[i])
	}
}

func readStepData(in io.Reader) (bikes []bike, speed int) {
	fmt.Fscan(in, &speed)
	d(speed)

	bikes = make([]bike, bikesNumber)

	for i := 0; i < bikesNumber; i++ {
		fmt.Fscan(in, &bikes[i].x, &bikes[i].y, &bikes[i].active)
		d(bikes[i])
	}

	return
}

func countBikes(bikes []bike) (n int) {
	n = 0
	for i := 0; i < bikesNumber; i++ {
		if bikes[i].active == 1 {
			n++
		}
	}
	return
}

func doMove(move string, bbikes []bike, speed int) ([]bike, int) {
	y := 0

	bikes := make([]bike, len(bbikes))
	copy(bikes, bbikes)

	d("move:", move)

	switch move {
	case "SPEED":
		speed++
		break
	case "SLOW":
		speed--
		break
	case "UP":
		y = -1
		break
	case "DOWN":
		y = 1
		break
	}

	for i := range bikes {
		if bikes[i].active == 0 {
			continue
		}

		if move != "JUMP" {
			if y != 0 {
				bikes[i].active = check(bikes[i].x+1, bikes[i].x+speed-1, bikes[i].y+y)
			}

			if bikes[i].active == 1 {
				bikes[i].active = check(bikes[i].x+1, bikes[i].x+speed-1, bikes[i].y)
			}
		}

		bikes[i].x += speed
		bikes[i].y += y

		if bikes[i].active == 1 && !isBikeFinished(bikes[i]) && tracks[bikes[i].y][bikes[i].x] == '0' {
			bikes[i].active = 0
		}
	}

	return bikes, speed
}

func findBestMove(bikes []bike, speed int, depth int) string {
	for _, m := range moves {

		if m == "SLOW" && speed < 2 {
			continue
		}

		d("***", depth)
		d("speed:", speed)
		for i := range bikes {
			d("bike", i, bikes[i])
		}

		b, s := doMove(m, bikes, speed)

		switch {
		case areAllBikesFinished(b):
			d("finished")
			return m
		case countBikes(b) < minBikes:
			d("failed")
			continue
		default:
			d("ok")
			nextMove := findBestMove(b, s, depth+1)
			if nextMove != "" {
				return m
			}
		}
	}

	return ""
}

func areAllBikesFinished(bikes []bike) bool {
	n := 0
	for _, b := range bikes {
		if isBikeFinished(b) {
			n++
		}
	}
	return n >= minBikes
}

func isBikeFinished(bike bike) bool {
	return bike.active == 1 && bike.x >= len(tracks[bike.y])
}

func check(x1 int, x2 int, y int) int {

	if y < 0 || y > 3 {
		return 0
	}

	for i := x1; i <= x2 && i < len(tracks[y]); i++ {
		if tracks[y][i] == '0' {
			return 0
		}
	}
	return 1
}
