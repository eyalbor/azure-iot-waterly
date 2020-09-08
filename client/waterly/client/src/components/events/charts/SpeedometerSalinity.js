import React from 'react'
import ReactSpeedometer from "react-d3-speedometer"

const SpeedometerSalinity = props => {
    return (
    <div className="ui center aligned compact segment">
        <h4 class="ui header">Current Avarege Salinity</h4>
        <ReactSpeedometer
            maxValue={2500}
            minValue={0}
            width={300}
            needleHeightRatio={0.6}
            value={props.avg}
            currentValueText={`Salinity : ${props.avg} mg/L` }
            segments={6}
            customSegmentStops={[0,100,250,400,2500]}
            startColor="blue"
            endColor="#e6e600"
            ringWidth={47}
            needleTransitionDuration={3333}
            needleTransition="easeElastic"
            needleColor={"#90f2ff"}
        />
    </div>
    )
}

export default SpeedometerSalinity;