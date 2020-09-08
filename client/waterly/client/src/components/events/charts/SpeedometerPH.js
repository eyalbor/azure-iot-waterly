import React from 'react'
import ReactSpeedometer from "react-d3-speedometer"
import Paper from '@material-ui/core/Paper';

const SpeedometerPH = props => {
    console.log(props)
    return (
    <div className="ui center aligned compact segment">
        <h4 class="ui header">Current Avarege Ph</h4>
        <ReactSpeedometer
            maxValue={14}
            width={400}
            needleHeightRatio={0.6}
            value={props.avg}
            currentValueText={`Ph: ${props.avg}` }
            segments={10}
            customSegmentStops={[0,1,2,3,4,5,6,7,8,9,10,11,12,13,14]}
            segmentColors={["#EE1C28","#F26724","#F6C611",
                "#F5EF16","#B5D333","#82C341",
                "#4DB749","#37A84C","#25B16E",
                "#0BB7B6","#4792CB","#664498","#452C84"
            ]}
            ringWidth={47}
            needleTransitionDuration={3333}
            needleTransition="easeElastic"
            needleColor={"#90f2ff"}
        />
    </div>
    )
}

export default SpeedometerPH;