package main

import (
	"fmt"
	"strconv"
)

type node struct {
	id       int
	money    int
	rooms    [2]int
	maxMoney int
}

func main() {
	var N int

	fmt.Scan(&N)

	nodes := make([]node, N)

	for i := 0; i < N; i++ {
		nodes[i].maxMoney = 0
		fmt.Scan(&nodes[i].id, &nodes[i].money)
		scanRoom(&nodes[i].rooms[0])
		scanRoom(&nodes[i].rooms[1])
	}

	money := max(nodes, 0)

	fmt.Println(money)
}

func max(nodes []node, index int) int {

	if index < 0 {
		return 0
	}

	if nodes[index].maxMoney > 0 {
		return nodes[index].maxMoney
	}

	maxMoney := 0
	for _, roomIndex := range nodes[index].rooms {
		maxMoney = maxInts(maxMoney, nodes[index].money+max(nodes, roomIndex))
	}

	nodes[index].maxMoney = maxMoney

	return maxMoney
}

func maxInts(a int, b int) int {
	if a > b {
		return a
	}

	return b
}

func scanRoom(room *int) {

	var roomStr string
	fmt.Scan(&roomStr)

	*room = -1
	if roomStr != "E" {
		*room, _ = strconv.Atoi(roomStr)
	}
}
