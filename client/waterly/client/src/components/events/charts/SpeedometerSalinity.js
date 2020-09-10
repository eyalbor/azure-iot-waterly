import React from 'react'
import ReactSpeedometer from "react-d3-speedometer"

const SpeedometerSalinity = props => {
    return (
    <div className="ui center aligned compact segment">
        <h4 class="ui header">Current Average Salinity</h4>
        <ReactSpeedometer
            maxValue={450}
            minValue={0}
            width={300}
            height={200}
            needleHeightRatio={0.6}
            value={props.avg}
            currentValueText={`Salinity : ${props.avg} mg/L` }
            segments={6}
            customSegmentStops={[0,50,100,150,200,250,300,350,400,450]}
            startColor="blue"
            endColor="brown"
            ringWidth={47}
            needleTransitionDuration={3333}
            needleTransition="easeElastic"
            needleColor={"#90f2ff"}
        />
    </div>
    )
}

export default SpeedometerSalinity;