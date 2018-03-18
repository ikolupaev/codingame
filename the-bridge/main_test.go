package main

import (
	"fmt"
	"os"
	"testing"

	"github.com/stretchr/testify/assert"
)

func TestReadInitAndStepData(t *testing.T) {
	f, err := os.Open("01-simple.txt")

	if err != nil {
		panic(err)
	}

	defer f.Close()

	dbg = os.Stderr

	readInitData(f)

	if bikesNumber != 1 {
		t.Error("wrong bikes number", bikesNumber)
	}

	if minBikes != 1 {
		t.Error("wrong min bikes number", minBikes)
	}

	if tracks[2] != "...........0.................." {
		t.Error("wrong line", tracks[2])
	}

	bikes, speed := readStepData(f)

	if speed != 0 {
		t.Error("speed error", speed)
	}

	if bikes[0].x != 0 || bikes[0].y != 2 || bikes[0].active != 1 {
		t.Error("wrong min bikes number", minBikes)
	}
}

func TestCheck(t *testing.T) {
	f, err := os.Open("01-simple.txt")

	if err != nil {
		panic(err)
	}
	defer f.Close()
	dbg = os.Stderr
	readInitData(f)
	assert.Equal(t, 1, check(0, 1, 0))
	assert.Equal(t, 1, check(0, len(tracks[0]), 0))
	assert.Equal(t, 0, check(0, len(tracks[0]), 2))
}

func TestDoFirstMove(t *testing.T) {
	bikes, speed := load("01-simple.txt")
	bikes, speed = doMove("SPEED", bikes, speed)

	assert.Equal(t, 1, speed, "speed is not correct")
	assert.Equal(t, 1, bikes[0].x, "x is not correct")
	assert.Equal(t, 2, bikes[0].y, "y is not correct")
	assert.Equal(t, 1, bikes[0].active, "active is not correct")
}

func TestDoSpeed(t *testing.T) {
	data := []struct {
		speed       int
		bike        bike
		resultSpeed int
		resultBike  bike
	}{
		{
			0,
			bike{0, 2, 1},
			1,
			bike{1, 2, 1},
		},
		{
			2,
			bike{10, 2, 1},
			3,
			bike{13, 2, 0},
		},
	}

	for _, c := range data {
		load("01-simple.txt")
		bikesNumber = 1
		bikes, speed := doMove("SPEED", []bike{c.bike}, c.speed)
		bike := bikes[0]
		assert.Equal(t, c.resultSpeed, speed, "speed is not correct")
		assert.Equal(t, c.resultBike.x, bike.x, "x is not correct")
		assert.Equal(t, c.resultBike.y, bike.y, "y is not correct")
		assert.Equal(t, c.resultBike.active, bike.active, "active is not correct")

	}
}

func TestBikesConsistancy(t *testing.T) {
	bikes, speed := load("01-simple.txt")
	b, s := doMove("SPEED", bikes, speed)

	assert.Equal(t, 0, speed)
	assert.Equal(t, 0, bikes[0].x)
	assert.Equal(t, 1, s)
	assert.Equal(t, 1, b[0].x)
}

func load(path string) (bikes []bike, speed int) {
	f, err := os.Open(path)

	if err != nil {
		panic(err)
	}
	defer f.Close()
	dbg = os.Stderr
	readInitData(f)

	bikes, speed = readStepData(f)
	return
}

func TestSimpleRun(t *testing.T) {

	bikes, speed := load("01-simple.txt")

	d("bikes:", bikesNumber)
	d("min:", minBikes)
	d("track:", tracks[0], len(tracks[0]))
	d("bikes[0]:", bikes[0])

	move := findBestMove(bikes, speed, 0)
	d(move)
	activeBikes := countBikes(bikes)
	fmt.Printf("active bikes: %v\n", activeBikes)
	if activeBikes < minBikes {
		t.Fail()
	}
}

func Test02Run(t *testing.T) {

	bikes, speed := load("02.txt")

	d("bikes:", bikesNumber)
	d("min:", minBikes)
	d("track:", tracks[0], len(tracks[0]))
	d("bikes[0]:", bikes[0])

	move := findBestMove(bikes, speed, 0)
	d(move)
	activeBikes := countBikes(bikes)
	fmt.Printf("active bikes: %v\n", activeBikes)
	if activeBikes < minBikes {
		t.Fail()
	}
}

func Test06Run(t *testing.T) {

	bikes, speed := load("06.txt")

	d("bikes:", bikesNumber)
	d("min:", minBikes)
	d("track:", tracks[0], len(tracks[0]))
	d("bikes[0]:", bikes[0])

	move := findBestMove(bikes, speed, 0)
	d(move)
	activeBikes := countBikes(bikes)
	fmt.Printf("active bikes: %v\n", activeBikes)
	if activeBikes < minBikes {
		t.Fail()
	}
}

func Test12Run(t *testing.T) {

	bikes, speed := load("12.txt")

	d("bikes:", bikesNumber)
	d("min:", minBikes)
	d("track:", tracks[0], len(tracks[0]))
	d("bikes[0]:", bikes[0])

	move := findBestMove(bikes, speed, 0)
	d(move)
	activeBikes := countBikes(bikes)
	fmt.Printf("active bikes: %v\n", activeBikes)
	if activeBikes < minBikes {
		t.Fail()
	}
}

/*
func TestSimpleRun1(t *testing.T) {

	f, err := os.Open("01-simple.txt")

	if err != nil {
		panic(err)
	}

	defer f.Close()

	readInitData(f)
	speed, bikes := readStepData(f)

	move := findBestMove(bikes, speed)

	if move != "SPEED" {
		t.Error(move)
	}
}
*/
